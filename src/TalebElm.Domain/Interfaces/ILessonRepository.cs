using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<Lesson?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default);
}
