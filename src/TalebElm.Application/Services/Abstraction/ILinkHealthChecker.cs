using System.Net;

namespace TalebElm.Application.Services.Abstraction;

public sealed record LinkCheckResult(
    bool IsHealthy,
    int? StatusCode = null,
    string? FailureReason = null);

public interface IDnsResolver
{
    Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress, CancellationToken ct = default);
}

public interface ILinkHealthChecker
{
    Task<LinkCheckResult> CheckAsync(string url, CancellationToken ct = default);
}
