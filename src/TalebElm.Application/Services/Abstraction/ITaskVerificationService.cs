using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ITaskVerificationService
{
    Task<VerificationOutcome> VerifyRepositoryAsync(
        Guid userId, Guid gitHubTaskId, string repositoryName,
        string? requiredJobName = null, CancellationToken ct = default);
}
