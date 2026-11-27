using System.Text;
using System.Xml.XPath;
using TalebElm.Api.Services;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class LessonService(IUnitOfWork unitOfWork, ICurrentUserService userService) : ILessonService
{
    public async Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(moduleId, ct);

        if (module is null || module.Track.Status != Domain.Enums.TrackStatus.Published)
            throw new NotFoundException($"Module with id '{moduleId}' was not found.");

        var userProgress = await unitOfWork.UserProgresses.GetByUserAndModuleAsync(CurrentUserId(), moduleId, ct);

        if (userProgress is null || userProgress.IsUnlocked == false)
            throw new ForbiddenExcpetion("You are not allowed to access this module.");

        var lessons = await unitOfWork.Lessons.GetByModuleIdAsync(moduleId,ct);

        return lessons
            .OrderBy(lesson => lesson.Order)
            .Select(lesson => new LessonResponse(lesson.Id, lesson.Title, lesson.ModuleId))
            .ToList();
    }

    public async Task<LessonDetailResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lesson = await unitOfWork.Lessons.GetByIdAsync(id,ct);

        if (lesson is null || lesson.Module.Track.Status != TrackStatus.Published)
        {
            throw new NotFoundException($"Lesson with id '{id}' was not found.");
        }

        var userProgress = await unitOfWork.UserProgresses.GetByUserAndModuleAsync(CurrentUserId(), lesson.ModuleId, ct);
        if (userProgress is null || userProgress.IsUnlocked == false)
            throw new ForbiddenExcpetion("You are not allowed to access this module.");

        var resources = await unitOfWork.LessonResources.GetByLessonIdAsync(id, ct);

        var resourceResponses = resources
            .OrderBy(resource => resource.Order)
            .Select(resource => new LessonResourceResponse(
                resource.Id,
                resource.Title,
                resource.Kind,
                resource.Url,
                resource.Citation,
                resource.Order))
            .ToList();

        return new LessonDetailResponse(
            lesson.Id,
            lesson.ModuleId,
            lesson.Title,
            lesson.Content,
            lesson.Order,
            resourceResponses);
    }

    private Guid CurrentUserId()
    {
        if (!Guid.TryParse(userService.UserId(), out var userId))
            throw new UnauthorizedException("An authenticated user is required.");

        return userId;
    }
}
