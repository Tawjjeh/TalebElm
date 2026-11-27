using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default);

    /// <summary>
    /// Loads a Module's Lessons ordered by <c>Order</c>, tracked, for ordering
    /// maintenance (<c>SiblingOrdering</c> in Application): the caller
    /// mutates the returned entities and saves once. The read
    /// <see cref="GetByModuleIdAsync"/> intentionally returns detached copies.
    /// </summary>
    Task<IReadOnlyList<Lesson>> GetByModuleIdForUpdateAsync(Guid moduleId, CancellationToken ct = default);
}
