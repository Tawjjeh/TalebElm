using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class LessonResourceRepository(AppDbContext context) : ILessonResourceRepository
{
    public async Task AddAsync(LessonResource entity)
    {
        await context.LessonResources.AddAsync(entity);
    }

    public async Task<IReadOnlyList<LessonResource>> GetAllAsync()
    {
        return await context.LessonResources.AsNoTracking().ToListAsync();
    }

    public async Task<LessonResource?> GetByIdAsync(Guid id)
    {
        return await context.LessonResources.FirstOrDefaultAsync(r => r.Id == id);
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
