using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class ModuleRepository(AppDbContext context) : IModuleRepository
{
    public async Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Modules.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Modules.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Module entity, CancellationToken ct = default)
    {
        await context.Modules.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId, CancellationToken ct = default)
    {
        return await context.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).AsNoTracking().ToListAsync(ct);
    }
}