using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class ModuleProblemRepository(AppDbContext context) : IModuleProblemRepository
{
    public async Task<ModuleProblem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Set<ModuleProblem>().FirstOrDefaultAsync(problem => problem.Id == id, ct);

    public async Task<IReadOnlyList<ModuleProblem>> GetAllAsync(CancellationToken ct = default)
        => await context.Set<ModuleProblem>().AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(ModuleProblem entity, CancellationToken ct = default)
        => await context.Set<ModuleProblem>().AddAsync(entity, ct);

    public async Task<IReadOnlyList<ModuleProblem>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
        => await context.Set<ModuleProblem>()
            .Where(problem => problem.ModuleId == moduleId)
            .OrderBy(problem => problem.Order)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> CountRequiredAsync(Guid moduleId, CancellationToken ct = default)
        => await context.Set<ModuleProblem>()
            .AsNoTracking()
            .CountAsync(problem => problem.ModuleId == moduleId && problem.IsRequired, ct);
}
