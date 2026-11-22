using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Infrastructure;
using TalebElm.Infrastructure.Services;

namespace TalebElm.Tests.UnitTests;

public class LinkHealthCheckerTests
{
    private static readonly IPAddress PublicIp = IPAddress.Parse("93.184.216.34");

    private static LinkHealthChecker CreateChecker(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler,
        IDnsResolver? dnsResolver = null,
        TimeSpan? timeout = null,
        TimeSpan? perHostSpacing = null,
        PerHostRequestSpacer? spacer = null)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler));
        return new LinkHealthChecker(
            httpClient,
            dnsResolver ?? new StubDnsResolver((_, _) => Task.FromResult<IPAddress[]>([PublicIp])),
            timeout ?? TimeSpan.FromSeconds(5),
            perHostSpacing ?? TimeSpan.Zero,
            spacer);
    }

    [Fact]
    public async Task CheckAsync_WhenHeadReturns200_ReturnsHealthyWithFixedUserAgent()
    {
        var methods = new List<HttpMethod>();
        string? observedUserAgent = null;

        var checker = CreateChecker((req, _) =>
        {
            methods.Add(req.Method);
            observedUserAgent = req.Headers.UserAgent.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var result = await checker.CheckAsync("https://example.com/docs");

        Assert.True(result.IsHealthy);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.FailureReason);
        Assert.Single(methods);
        Assert.Equal(HttpMethod.Head, methods[0]);
        Assert.Contains(LinkHealthChecker.FixedUserAgent, observedUserAgent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/relative/path")]
    public async Task CheckAsync_WhenUrlInvalid_ReturnsInvalidUrlWithoutHttpCall(string url)
    {
        var httpCalled = false;
        var checker = CreateChecker((_, _) =>
        {
            httpCalled = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var result = await checker.CheckAsync(url);

        Assert.False(result.IsHealthy);
        Assert.Equal("InvalidUrl", result.FailureReason);
        Assert.False(httpCalled);
    }

    [Theory]
    [InlineData("ftp://example.com/file")]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    public async Task CheckAsync_WhenSchemeIsNotHttpOrHttps_RejectsWithoutHttpCall(string url)
    {
        var httpCalled = false;
        var checker = CreateChecker((_, _) =>
        {
            httpCalled = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var result = await checker.CheckAsync(url);

        Assert.False(result.IsHealthy);
        Assert.Equal("UnsupportedScheme", result.FailureReason);
        Assert.False(httpCalled);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("127.255.0.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.254")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.169.254")]
    [InlineData("100.64.0.1")]
    [InlineData("198.18.0.1")]
    [InlineData("192.0.2.1")]
    [InlineData("0.0.0.0")]
    [InlineData("::1")]
    [InlineData("fc00::1")]
    [InlineData("fd12:3456::1")]
    [InlineData("fe80::1")]
    [InlineData("::ffff:127.0.0.1")]
    [InlineData("::ffff:169.254.169.254")]
    [InlineData("64:ff9b::127.0.0.1")]
    [InlineData("2002:7f00:0001::")]
    [InlineData("2001:0::1")]
    public async Task CheckAsync_WhenDnsResolvesToUnsafeIp_BlocksSsrfBeforeHttpCall(string resolvedIp)
    {
        var httpCalled = false;
        var dns = new StubDnsResolver((_, _) => Task.FromResult<IPAddress[]>([IPAddress.Parse(resolvedIp)]));
        var checker = CreateChecker((_, _) =>
        {
            httpCalled = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }, dns);

        var result = await checker.CheckAsync("https://internal.example.com/secret");

        Assert.False(result.IsHealthy);
        Assert.Equal("SsrfBlocked", result.FailureReason);
        Assert.False(httpCalled);
    }

    [Theory]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotImplemented)]
    public async Task CheckAsync_WhenHeadReturns405Or403Or501_RetriesOnceWithBoundedGet(HttpStatusCode headStatus)
    {
        var methods = new List<HttpMethod>();
        var checker = CreateChecker((req, _) =>
        {
            methods.Add(req.Method);
            if (req.Method == HttpMethod.Head)
                return Task.FromResult(new HttpResponseMessage(headStatus));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new string('x', 8192))
            });
        });

        var result = await checker.CheckAsync("https://example.com/article");

        Assert.True(result.IsHealthy);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal([HttpMethod.Head, HttpMethod.Get], methods);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, 404)]
    [InlineData(HttpStatusCode.Gone, 410)]
    [InlineData(HttpStatusCode.InternalServerError, 500)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
    public async Task CheckAsync_WhenStatusIs404Or410Or5xx_ReturnsUnhealthy(HttpStatusCode status, int expectedCode)
    {
        var checker = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(status)));

        var result = await checker.CheckAsync("https://example.com/broken");

        Assert.False(result.IsHealthy);
        Assert.Equal(expectedCode, result.StatusCode);
        Assert.Equal($"HttpStatus_{expectedCode}", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenRedirectsWithinLimit_FollowsAndReturnsHealthy()
    {
        var hop = 0;
        var checker = CreateChecker((_, _) =>
        {
            hop++;
            if (hop <= 5)
            {
                var redirect = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
                redirect.Headers.Location = new Uri($"https://example.com/hop-{hop}");
                return Task.FromResult(redirect);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var result = await checker.CheckAsync("https://example.com/start");

        Assert.True(result.IsHealthy);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(6, hop);
    }

    [Fact]
    public async Task CheckAsync_WhenRedirectMissingLocation_ReturnsRedirectMissingLocation()
    {
        var checker = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Found)));

        var result = await checker.CheckAsync("https://example.com/no-loc");

        Assert.False(result.IsHealthy);
        Assert.Equal(302, result.StatusCode);
        Assert.Equal("RedirectMissingLocation", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenRelativeRedirect_ResolvesAndFollowsSafely()
    {
        var uris = new List<string>();
        var checker = CreateChecker((req, _) =>
        {
            uris.Add(req.RequestUri!.ToString());
            if (uris.Count == 1)
            {
                var redirect = new HttpResponseMessage(HttpStatusCode.Found);
                redirect.Headers.Location = new Uri("/final-page", UriKind.Relative);
                return Task.FromResult(redirect);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var result = await checker.CheckAsync("https://example.com/start");

        Assert.True(result.IsHealthy);
        Assert.Equal(["https://example.com/start", "https://example.com/final-page"], uris);
    }

    [Fact]
    public async Task CheckAsync_WhenRedirectsExceedFive_ReturnsTooManyRedirects()
    {
        var checker = CreateChecker((_, _) =>
        {
            var redirect = new HttpResponseMessage(HttpStatusCode.Found);
            redirect.Headers.Location = new Uri("https://example.com/loop");
            return Task.FromResult(redirect);
        });

        var result = await checker.CheckAsync("https://example.com/loop");

        Assert.False(result.IsHealthy);
        Assert.Equal("TooManyRedirects", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenRedirectPointsToPrivateIp_BlocksSsrfBeforeFollowing()
    {
        var requestedHosts = new List<string>();
        var dns = new StubDnsResolver((host, _) =>
            Task.FromResult<IPAddress[]>(host == "public.example.com" ? [PublicIp] : [IPAddress.Loopback]));

        var checker = CreateChecker((req, _) =>
        {
            requestedHosts.Add(req.RequestUri!.Host);
            var redirect = new HttpResponseMessage(HttpStatusCode.Found);
            redirect.Headers.Location = new Uri("http://internal.local/admin");
            return Task.FromResult(redirect);
        }, dns);

        var result = await checker.CheckAsync("https://public.example.com/go");

        Assert.False(result.IsHealthy);
        Assert.Equal("SsrfBlocked", result.FailureReason);
        Assert.Single(requestedHosts);
        Assert.Equal("public.example.com", requestedHosts[0]);
    }

    [Fact]
    public async Task CheckAsync_WhenDnsThrows_ReturnsDnsFailure()
    {
        var dns = new StubDnsResolver((_, _) => throw new SocketException());
        var checker = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), dns);

        var result = await checker.CheckAsync("https://nonexistent.invalid");

        Assert.False(result.IsHealthy);
        Assert.Equal("DnsFailure", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenDnsTimesOut_ReturnsTimeout()
    {
        var dns = new StubDnsResolver(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return [PublicIp];
        });
        var checker = CreateChecker(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
            dns,
            timeout: TimeSpan.FromMilliseconds(50));

        var result = await checker.CheckAsync("https://slow-dns.example.com");

        Assert.False(result.IsHealthy);
        Assert.Equal("Timeout", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenNetworkOrTlsThrows_ReturnsNetworkOrTlsFailure()
    {
        var checker = CreateChecker((_, _) => throw new HttpRequestException("TLS handshake failed"));

        var result = await checker.CheckAsync("https://bad-tls.example.com");

        Assert.False(result.IsHealthy);
        Assert.Equal("NetworkOrTlsFailure", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_WhenRequestTimesOut_ReturnsTimeout()
    {
        var checker = CreateChecker(
            async (_, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            },
            timeout: TimeSpan.FromMilliseconds(50));

        var result = await checker.CheckAsync("https://slow.example.com");

        Assert.False(result.IsHealthy);
        Assert.Equal("Timeout", result.FailureReason);
    }

    [Fact]
    public async Task CheckAsync_ConcurrentCallsToSameHost_EnforcesAtomicPerHostSpacing()
    {
        var sharedSpacer = new PerHostRequestSpacer(TimeSpan.FromMilliseconds(60));
        var checker1 = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), perHostSpacing: TimeSpan.FromMilliseconds(60), spacer: sharedSpacer);
        var checker2 = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), perHostSpacing: TimeSpan.FromMilliseconds(60), spacer: sharedSpacer);
        var checker3 = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), perHostSpacing: TimeSpan.FromMilliseconds(60), spacer: sharedSpacer);

        var sw = Stopwatch.StartNew();
        await Task.WhenAll(
            checker1.CheckAsync("https://spaced.example.com/1"),
            checker2.CheckAsync("https://spaced.example.com/2"),
            checker3.CheckAsync("https://spaced.example.com/3"));
        sw.Stop();

        Assert.True(sw.Elapsed >= TimeSpan.FromMilliseconds(100), $"Expected >= 100ms spacing, actual {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void AddInfrastructure_RegistersSingletonSpacerAndLinkHealthChecker()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TalebElm"] = "Data Source=:memory:"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(config);

        using var provider = services.BuildServiceProvider();
        var spacer1 = provider.GetRequiredService<PerHostRequestSpacer>();
        var spacer2 = provider.GetRequiredService<PerHostRequestSpacer>();
        var checker = provider.GetService<ILinkHealthChecker>();

        Assert.Same(spacer1, spacer2);
        Assert.NotNull(checker);
        Assert.IsType<LinkHealthChecker>(checker);
    }

    private sealed class StubDnsResolver(Func<string, CancellationToken, Task<IPAddress[]>> resolver) : IDnsResolver
    {
        public Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress, CancellationToken ct = default)
            => resolver(hostNameOrAddress, ct);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}
