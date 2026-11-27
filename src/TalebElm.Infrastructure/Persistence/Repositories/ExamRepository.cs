using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class ExamRepository(AppDbContext context) : IExamRepository
{
    public async Task AddAsync(Exam entity, CancellationToken ct = default)
    {
        await context.Set<Exam>().AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Set<Exam>().AsNoTracking().ToListAsync(ct);
    }

    public async Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<Exam>().FindAsync([id], ct);
    }

    public async Task<Exam?> GetWithQuestionsAsync(Guid examId, CancellationToken ct = default)
    {
        return await context.Set<Exam>()
            .Include(e => e.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == examId, ct);
    }

    public async Task<Exam?> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default)
    {
        return await context.Set<Exam>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ModuleId == moduleId, ct);
    }

    public Task RemoveAsync(Exam exam, CancellationToken ct = default)
    {
        // DeleteBehavior.Restrict on both FKs, so dependents go first.
        foreach (var question in exam.Questions)
            context.Set<ExamOption>().RemoveRange(question.Options);

        context.Set<ExamQuestion>().RemoveRange(exam.Questions);
        context.Set<Exam>().Remove(exam);
        return Task.CompletedTask;
    }

    public async Task AddOptionAsync(ExamQuestion question, ExamOption option, CancellationToken ct = default)
    {
        option.ExamQuestionId = question.Id;
        await context.Set<ExamOption>().AddAsync(option, ct);
    }

    public Task RemoveOptionAsync(ExamOption option, CancellationToken ct = default)
    {
        context.Set<ExamOption>().Remove(option);
        return Task.CompletedTask;
    }
}
