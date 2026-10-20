using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId);
}
