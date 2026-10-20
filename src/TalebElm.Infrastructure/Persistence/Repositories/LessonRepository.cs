using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class LessonRepository(AppDbContext context) : ILessonRepository
{
    public async Task<Lesson?> GetByIdAsync(Guid id)
        => await context.Lessons.FirstOrDefaultAsync(lesson => lesson.Id == id);

    public async Task<IReadOnlyList<Lesson>> GetAllAsync()
        => await context.Lessons.AsNoTracking().ToListAsync();

    public async Task AddAsync(Lesson entity)
        => await context.Lessons.AddAsync(entity);

    public async Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId)
        => await context.Lessons
            .Where(lesson => lesson.ModuleId == moduleId)
            .OrderBy(lesson => lesson.Order)
            .AsNoTracking()
            .ToListAsync();
}
