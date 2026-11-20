using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IProblemVerifier
{
    Task<ProblemVerificationResult> VerifyAsync(
        Guid userId, Guid moduleProblemId, CancellationToken ct = default);
}
