using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class LessonRepository(AppDbContext context) : ILessonRepository
{
    public async Task<Lesson?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Lessons.FirstOrDefaultAsync(lesson => lesson.Id == id, ct);

    public async Task<IReadOnlyList<Lesson>> GetAllAsync(CancellationToken ct = default)
        => await context.Lessons.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Lesson entity, CancellationToken ct = default)
        => await context.Lessons.AddAsync(entity, ct);

    public async Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default)
        => await context.Lessons
            .Where(lesson => lesson.ModuleId == moduleId)
            .OrderBy(lesson => lesson.Order)
            .AsNoTracking()
            .ToListAsync(ct);
}
