using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IUserProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ProgressResponse>> GetByTrackAsync(Guid userId, Guid trackId, CancellationToken ct = default);
    Task<ProgressResponse> AdminUnlockAsync(Guid adminId, AdminUnlockModuleRequest request, CancellationToken ct = default);
}
