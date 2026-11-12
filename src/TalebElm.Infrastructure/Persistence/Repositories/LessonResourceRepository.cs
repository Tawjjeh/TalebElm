using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class LessonResourceRepository(AppDbContext context) : ILessonResourceRepository
{
    public async Task AddAsync(LessonResource entity, CancellationToken ct = default)
    {
        await context.LessonResources.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<LessonResource>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.LessonResources.AsNoTracking().ToListAsync(ct);
    }

    public async Task<LessonResource?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.LessonResources.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<LessonResource>> GetByLessonIdAsync(Guid lessonId)
    {
        return await context.LessonResources
            .AsNoTracking()
            .Where(r => r.LessonId == lessonId)
            .OrderBy(r => r.Order)
            .ToListAsync();
    }
}
