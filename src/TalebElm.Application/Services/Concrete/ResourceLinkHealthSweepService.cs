using Microsoft.Extensions.Logging;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public sealed class ResourceLinkHealthSweepService(
    IUnitOfWork unitOfWork,
    ILinkHealthChecker linkHealthChecker,
    ILogger<ResourceLinkHealthSweepService> logger) : IResourceLinkHealthSweepService
{
    public const int FailureThreshold = 3;
    private const int MaxConcurrency = 4;

    public async Task<LinkHealthSweepSummary> RunSweepAsync(CancellationToken ct = default)
    {
        var resources = await unitOfWork.Resources.GetForHealthCheckAsync(ct);
        var checkable = resources
            .Where(r => !string.IsNullOrWhiteSpace(r.Url))
            .ToList();

        if (checkable.Count == 0)
            return new LinkHealthSweepSummary(0, 0, 0, 0, 0);

        using var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        var probeTasks = checkable.Select(async resource =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var result = await linkHealthChecker.CheckAsync(resource.Url!, ct);
                return (Resource: resource, Result: result);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var outcomes = await Task.WhenAll(probeTasks);

        var healthyCount = 0;
        var failedCount = 0;
        var markedBrokenCount = 0;
        var recoveredCount = 0;
        var checkedAt = DateTimeOffset.UtcNow;

        foreach (var (resource, result) in outcomes)
        {
            resource.LastCheckedAt = checkedAt;

            if (result.IsHealthy)
            {
                healthyCount++;
                resource.ConsecutiveFailureCount = 0;

                if (resource.Status == ResourceStatus.Broken)
                {
                    resource.Status = ResourceStatus.Approved;
                    recoveredCount++;
                }
            }
            else
            {
                failedCount++;
                resource.ConsecutiveFailureCount++;

                logger.LogInformation(
                    "Resource {ResourceId} link health check failed ({FailureCount}/{Threshold}): {Reason} (StatusCode={StatusCode})",
                    resource.Id,
                    resource.ConsecutiveFailureCount,
                    FailureThreshold,
                    result.FailureReason,
                    result.StatusCode);

                if (resource.ConsecutiveFailureCount >= FailureThreshold && resource.Status != ResourceStatus.Broken)
                {
                    resource.Status = ResourceStatus.Broken;
                    markedBrokenCount++;
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new LinkHealthSweepSummary(
            outcomes.Length,
            healthyCount,
            failedCount,
            markedBrokenCount,
            recoveredCount);
    }
}
