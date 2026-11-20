using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ITaskVerificationService
{
    Task<TaskVerificationResult> VerifyRepositoryAsync(
        Guid userId, Guid gitHubTaskId, string repositoryName,
        string? requiredJobName = null, CancellationToken ct = default);
}
