using Microsoft.Extensions.Logging;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class UserProgressService : IUserProgressService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserProgressService>? _logger;

    public UserProgressService(IUnitOfWork unitOfWork, ILogger<UserProgressService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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

    public async Task<ProgressResponse> AdminUnlockAsync(Guid adminId, AdminUnlockModuleRequest request, CancellationToken ct = default)
    {
        var learner = await _unitOfWork.Users.GetByIdAsync(request.LearnerId, ct);
        if (learner is null || learner.Role != UserRole.Student)
            throw new NotFoundException($"Learner with id '{request.LearnerId}' was not found.");

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleId, ct);
        if (module is null)
            throw new NotFoundException($"Module with id '{request.ModuleId}' was not found.");

        var progress = await _unitOfWork.UserProgresses.GetByUserAndModuleAsync(request.LearnerId, request.ModuleId, ct);

        string outcome;
        if (progress is null)
        {
            progress = new UserProgress
            {
                UserId = request.LearnerId,
                ModuleId = request.ModuleId,
                IsUnlocked = true,
                PassedExam = false,
                Score = 0
            };
            await _unitOfWork.UserProgresses.AddAsync(progress, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            outcome = "created";
        }
        else if (!progress.IsUnlocked)
        {
            // Unlock in place: Score, PassedExam, and CompletedRequiredProblems stay untouched.
            progress.IsUnlocked = true;
            await _unitOfWork.SaveChangesAsync(ct);
            outcome = "unlocked";
        }
        else
        {
            outcome = "already-unlocked";
        }

        // Audit event: identifiers only, no PII (no names or emails).
        _logger?.LogInformation(
            "Admin manual module unlock: admin {AdminId} learner {LearnerId} module {ModuleId} outcome {Outcome}",
            adminId, request.LearnerId, request.ModuleId, outcome);

        return new ProgressResponse(progress.ModuleId, progress.IsUnlocked, progress.PassedExam, progress.Score);
    }
}
