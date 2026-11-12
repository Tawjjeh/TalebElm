using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class ExamRepository(AppDbContext context) : IExamRepository
{
    public async Task AddAsync(Exam entity)
    {
        await context.Set<Exam>().AddAsync(entity);
    }

    public async Task<IReadOnlyList<Exam>> GetAllAsync()
    {
        return await context.Set<Exam>().AsNoTracking().ToListAsync();
    }

    public async Task<Exam?> GetByIdAsync(Guid id)
    {
        return await context.Set<Exam>().FindAsync(id);
    }

    public async Task<Exam?> GetWithQuestionsAsync(Guid examId)
    {
        return await context.Set<Exam>()
            .Include(e => e.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == examId);
    }

    public async Task<Exam?> GetByModuleIdAsync(Guid moduleId)
    {
        return await context.Set<Exam>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ModuleId == moduleId);
    }
}
