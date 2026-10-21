
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ILessonService
{
    Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId);

    Task<LessonDetailResponse> GetByIdAsync(Guid id);
}
