namespace TalebElm.Application.Services.Abstraction;

public sealed record LinkHealthSweepSummary(
    int CheckedCount,
    int HealthyCount,
    int FailedCount,
    int MarkedBrokenCount,
    int RecoveredCount);

public interface IResourceLinkHealthSweepService
{
    Task<LinkHealthSweepSummary> RunSweepAsync(CancellationToken ct = default);
}
