using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class TrackEnrollmentService(IUnitOfWork unitOfWork) : ITrackEnrollmentService
{
    public async Task<EnrollmentResponse> EnrollAsync(Guid userId, Guid trackId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with id '{userId}' was not found.");

        var track = await unitOfWork.Tracks.GetByIdAsync(trackId);
        if (track is null)
            throw new NotFoundException($"Track with id '{trackId}' was not found.");

        var existingEnrollment = await unitOfWork.TrackEnrollments.GetAllAsync();
        if (existingEnrollment.Any(enrollment => enrollment.UserId == userId && enrollment.TrackId == trackId))
            throw new ConflictException("The user has already enrolled in this track.");

        var modules = await unitOfWork.Modules.GetByTrackIdAsync(trackId);
        if (modules.Count == 0)
            throw new ValidationException("The track has no modules to enroll in.");

        var firstModule = modules.OrderBy(module => module.Order).First();

        var enrollment = new TrackEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TrackId = trackId,
            EnrolledAt = DateTimeOffset.UtcNow
        };

        var progress = new UserProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ModuleId = firstModule.Id,
            IsUnlocked = true
        };

        await unitOfWork.TrackEnrollments.AddAsync(enrollment);
        await unitOfWork.UserProgresses.AddAsync(progress);
        await unitOfWork.SaveChangesAsync();

        return new EnrollmentResponse(enrollment.Id, enrollment.TrackId, enrollment.EnrolledAt);
    }
}
