using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ILessonResourceRepository : IRepository<LessonResource>
{
    Task<IReadOnlyList<LessonResource>> GetByLessonIdAsync(Guid lessonId);
}
