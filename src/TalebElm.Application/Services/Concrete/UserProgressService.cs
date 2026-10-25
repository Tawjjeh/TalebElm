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

    public async Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(Guid userId)
    {
        var progress = await _unitOfWork.UserProgresses.GetAllAsync();
        return progress
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Module?.Order ?? int.MaxValue)
            .Select(p => new ProgressResponse(p.ModuleId, p.IsUnlocked, p.PassedExam, p.Score))
            .ToList();
    }

    public async Task<IReadOnlyList<ProgressResponse>> GetByTrackAsync(Guid userId, Guid trackId)
    {
        var progress = await _unitOfWork.UserProgresses.GetByUserAndTrackAsync(userId, trackId);
        return progress
            .Select(p => new ProgressResponse(p.ModuleId, p.IsUnlocked, p.PassedExam, p.Score))
            .ToList();
    }
}
