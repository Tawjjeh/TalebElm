
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ILessonService
{
    Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);

    Task<LessonDetailResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
}
