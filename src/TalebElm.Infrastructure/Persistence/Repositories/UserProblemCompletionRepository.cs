using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class UserProblemCompletionRepository(AppDbContext context) : IUserProblemCompletionRepository
{
    public async Task<UserProblemCompletion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.UserProblemCompletions.FirstOrDefaultAsync(completion => completion.Id == id, ct);

    public async Task<IReadOnlyList<UserProblemCompletion>> GetAllAsync(CancellationToken ct = default)
        => await context.UserProblemCompletions.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(UserProblemCompletion entity, CancellationToken ct = default)
        => await context.UserProblemCompletions.AddAsync(entity, ct);

    public async Task<UserProblemCompletion?> GetByUserAndProblemAsync(
        Guid userId,
        Guid moduleProblemId,
        CancellationToken ct = default)
        => await context.UserProblemCompletions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                completion => completion.UserId == userId && completion.ModuleProblemId == moduleProblemId,
                ct);

    public async Task<int> CountForUserInModuleAsync(
        Guid userId,
        Guid moduleId,
        CancellationToken ct = default)
        => await context.UserProblemCompletions
            .AsNoTracking()
            .Where(completion => completion.UserId == userId && completion.ModuleProblem.ModuleId == moduleId)
            .Select(completion => completion.ModuleProblemId)
            .Distinct()
            .CountAsync(ct);
}
