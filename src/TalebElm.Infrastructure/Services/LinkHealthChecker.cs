using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Infrastructure.Services;

public sealed class SystemDnsResolver : IDnsResolver
{
    public Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress, CancellationToken ct = default)
        => Dns.GetHostAddressesAsync(hostNameOrAddress, ct);
}

public sealed class PerHostRequestSpacer(TimeSpan? defaultSpacing = null)
{
    private readonly TimeSpan _defaultSpacing = defaultSpacing ?? TimeSpan.FromMilliseconds(250);
    private readonly ConcurrentDictionary<string, HostSlot> _slots = new(StringComparer.OrdinalIgnoreCase);

    public async Task WaitTurnAsync(string host, TimeSpan? spacingOverride, CancellationToken ct)
    {
        var spacing = spacingOverride ?? _defaultSpacing;
        if (spacing <= TimeSpan.Zero)
            return;

        var slot = _slots.GetOrAdd(host, static _ => new HostSlot());
        await slot.Gate.WaitAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (slot.LastRequestAt is { } last)
            {
                var elapsed = now - last;
                if (elapsed < spacing)
                    await Task.Delay(spacing - elapsed, ct);
            }

            slot.LastRequestAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            slot.Gate.Release();
        }
    }

    private sealed class HostSlot
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);
        public DateTimeOffset? LastRequestAt { get; set; }
    }
}

public sealed class LinkHealthChecker : ILinkHealthChecker
{
    public const string FixedUserAgent = "TalebElm-LinkChecker/1.0";
    private const int MaxRedirects = 5;
    private const int MaxGetBytes = 4096;
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(10);

    private readonly HttpClient _httpClient;
    private readonly IDnsResolver _dnsResolver;
    private readonly PerHostRequestSpacer _spacer;
    private readonly TimeSpan _requestTimeout;
    private readonly TimeSpan? _perHostSpacingOverride;

    [ActivatorUtilitiesConstructor]
    public LinkHealthChecker(HttpClient httpClient, IDnsResolver dnsResolver, PerHostRequestSpacer spacer)
        : this(httpClient, dnsResolver, spacer, DefaultRequestTimeout, perHostSpacingOverride: null)
    {
    }

    public LinkHealthChecker(
        HttpClient httpClient,
        IDnsResolver dnsResolver,
        TimeSpan requestTimeout,
        TimeSpan perHostSpacing,
        PerHostRequestSpacer? spacer = null)
        : this(httpClient, dnsResolver, spacer ?? new PerHostRequestSpacer(perHostSpacing), requestTimeout, perHostSpacing)
    {
    }

    private LinkHealthChecker(
        HttpClient httpClient,
        IDnsResolver dnsResolver,
        PerHostRequestSpacer spacer,
        TimeSpan requestTimeout,
        TimeSpan? perHostSpacingOverride)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(dnsResolver);
        ArgumentNullException.ThrowIfNull(spacer);

        _httpClient = httpClient;
        _dnsResolver = dnsResolver;
        _spacer = spacer;
        _requestTimeout = requestTimeout;
        _perHostSpacingOverride = perHostSpacingOverride;
    }

    public async Task<LinkCheckResult> CheckAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)
            || url.TrimStart().StartsWith('/')
            || !Uri.TryCreate(url, UriKind.Absolute, out var currentUri))
            return new LinkCheckResult(false, null, "InvalidUrl");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_requestTimeout);
        var linkedToken = timeoutCts.Token;

        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            if (currentUri.Scheme != Uri.UriSchemeHttp && currentUri.Scheme != Uri.UriSchemeHttps)
                return new LinkCheckResult(false, null, "UnsupportedScheme");

            HttpResponseMessage? response = null;
            try
            {
                var ssrfFailure = await ValidateHostSafetyAsync(currentUri.DnsSafeHost, linkedToken);
                if (ssrfFailure is not null)
                    return new LinkCheckResult(false, null, ssrfFailure);

                await _spacer.WaitTurnAsync(currentUri.DnsSafeHost, _perHostSpacingOverride, linkedToken);

                response = await SendRequestAsync(HttpMethod.Head, currentUri, linkedToken);

                if (ShouldFallbackToGet(response.StatusCode))
                {
                    response.Dispose();
                    response = await SendRequestAsync(HttpMethod.Get, currentUri, linkedToken);
                    await ReadBoundedBodyAsync(response, linkedToken);
                }

                var code = (int)response.StatusCode;

                if (IsRedirect(code))
                {
                    var location = response.Headers.Location;
                    if (location is null)
                        return new LinkCheckResult(false, code, "RedirectMissingLocation");

                    if (redirectCount == MaxRedirects)
                        return new LinkCheckResult(false, code, "TooManyRedirects");

                    currentUri = location.IsAbsoluteUri ? location : new Uri(currentUri, location);
                    continue;
                }

                if (code is >= 200 and < 400)
                    return new LinkCheckResult(true, code, null);

                return new LinkCheckResult(false, code, $"HttpStatus_{code}");
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return new LinkCheckResult(false, null, "Timeout");
            }
            catch (HttpRequestException)
            {
                return new LinkCheckResult(false, null, "NetworkOrTlsFailure");
            }
            finally
            {
                response?.Dispose();
            }
        }

        return new LinkCheckResult(false, null, "TooManyRedirects");
    }

    public static async ValueTask<Stream> ConnectWithSsrfGuardAsync(
        SocketsHttpConnectionContext context,
        IDnsResolver dnsResolver,
        CancellationToken ct)
    {
        var addresses = IPAddress.TryParse(context.DnsEndPoint.Host, out var literalIp)
            ? [literalIp]
            : await dnsResolver.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);

        if (addresses.Length == 0 || addresses.Any(IsUnsafeIpAddress))
            throw new HttpRequestException("Connection blocked by SSRF guard.");

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(addresses[0], context.DnsEndPoint.Port, ct);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, Uri uri, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.UserAgent.ParseAdd(FixedUserAgent);
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private static async Task ReadBoundedBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var buffer = new byte[MaxGetBytes];
        _ = await stream.ReadAsync(buffer.AsMemory(0, MaxGetBytes), ct);
    }

    private static bool ShouldFallbackToGet(HttpStatusCode code)
        => code is HttpStatusCode.MethodNotAllowed or HttpStatusCode.Forbidden or HttpStatusCode.NotImplemented;

    private static bool IsRedirect(int statusCode)
        => statusCode is 301 or 302 or 303 or 307 or 308;

    private async Task<string?> ValidateHostSafetyAsync(string host, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "InvalidHost";

        IPAddress[] addresses;
        if (IPAddress.TryParse(host, out var literalIp))
        {
            addresses = [literalIp];
        }
        else
        {
            try
            {
                addresses = await _dnsResolver.GetHostAddressesAsync(host, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return "DnsFailure";
            }
        }

        if (addresses.Length == 0)
            return "DnsFailure";

        foreach (var ip in addresses)
        {
            if (IsUnsafeIpAddress(ip))
                return "SsrfBlocked";
        }

        return null;
    }

    public static bool IsUnsafeIpAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return true;

        if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.IPv6None))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 0                                             // 0.0.0.0/8 (Unspecified)
                || bytes[0] == 10                                            // 10.0.0.0/8 (RFC 1918)
                || bytes[0] == 127                                           // 127.0.0.0/8 (Loopback)
                || (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)    // 100.64.0.0/10 (CGNAT)
                || (bytes[0] == 169 && bytes[1] == 254)                      // 169.254.0.0/16 (Link-local & Cloud metadata)
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)     // 172.16.0.0/12 (RFC 1918)
                || (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0)       // 192.0.0.0/24 (IETF Protocol)
                || (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)       // 192.0.2.0/24 (TEST-NET-1)
                || (bytes[0] == 192 && bytes[1] == 168)                      // 192.168.0.0/16 (RFC 1918)
                || (bytes[0] == 198 && (bytes[1] == 18 || bytes[1] == 19))   // 198.18.0.0/15 (Benchmarking)
                || (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)    // 198.51.100.0/24 (TEST-NET-2)
                || (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)     // 203.0.113.0/24 (TEST-NET-3)
                || bytes[0] >= 224;                                          // Multicast & Reserved
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
                return true;

            var bytes = address.GetAddressBytes();
            // Unique Local Address (fc00::/7)
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;
            // NAT64 well-known prefix (64:ff9b::/96)
            if (bytes[0] == 0x00 && bytes[1] == 0x64 && bytes[2] == 0xFF && bytes[3] == 0x9B)
                return true;
            // 6to4 (2002::/16)
            if (bytes[0] == 0x20 && bytes[1] == 0x02)
                return true;
            // Teredo (2001:0000::/32)
            if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x00 && bytes[3] == 0x00)
                return true;
        }

        return false;
    }
}
