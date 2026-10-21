using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class LessonService(IUnitOfWork unitOfWork) : ILessonService
{
    public async Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId)
    {
        if (await unitOfWork.Modules.GetByIdAsync(moduleId) is null)
        {
            throw new NotFoundException($"Module with id '{moduleId}' was not found.");
        }

        var lessons = await unitOfWork.Lessons.GetByModuleIdAsync(moduleId);

        return lessons
            .OrderBy(lesson => lesson.Order)
            .Select(lesson => new LessonResponse(lesson.Id, lesson.Title, lesson.ModuleId))
            .ToList();
    }

    public async Task<LessonDetailResponse> GetByIdAsync(Guid id)
    {
        var lesson = await unitOfWork.Lessons.GetByIdAsync(id);

        if (lesson is null)
        {
            throw new NotFoundException($"Lesson with id '{id}' was not found.");
        }

        var resources = await unitOfWork.LessonResources.GetByLessonIdAsync(id);

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
}
