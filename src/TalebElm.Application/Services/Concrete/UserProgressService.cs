using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class UserProgressService : IUserProgressService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserProgressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(Guid userId, CancellationToken ct = default)
    {
        var progress = await _unitOfWork.UserProgresses.GetAllAsync(ct);
        return progress
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Module?.Order ?? int.MaxValue)
            .Select(p => new ProgressResponse(p.ModuleId, p.IsUnlocked, p.PassedExam, p.Score))
            .ToList();
    }

    public async Task<IReadOnlyList<ProgressResponse>> GetByTrackAsync(Guid userId, Guid trackId, CancellationToken ct = default)
    {
        var progress = await _unitOfWork.UserProgresses.GetByUserAndTrackAsync(userId, trackId, ct);
        return progress
            .Select(p => new ProgressResponse(p.ModuleId, p.IsUnlocked, p.PassedExam, p.Score))
            .ToList();
    }
}
