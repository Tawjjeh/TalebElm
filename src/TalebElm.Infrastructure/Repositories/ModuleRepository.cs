using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class ModuleRepository(AppDbContext context) : IModuleRepository
{
    public async Task<Module?> GetByIdAsync(Guid id)
    {
        return await context.Modules.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IReadOnlyList<Module>> GetAllAsync()
    {
        return await context.Modules.ToListAsync();
    }

    public async Task AddAsync(Module entity)
    {
        await context.Modules.AddAsync(entity);
    }

    public async Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId)
    {
        return await context.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).ToListAsync();
    }
}