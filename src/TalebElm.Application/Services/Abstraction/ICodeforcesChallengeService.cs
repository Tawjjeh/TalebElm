using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ICodeforcesChallengeService
{
    Task<ExternalAccountResponse> GenerateChallengeAsync(Guid userId, string handle, CancellationToken ct = default);
}
