using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class UserProgressRepository(AppDbContext context) : IUserProgressRepository
{
    public async Task AddAsync(UserProgress entity, CancellationToken ct = default)
    {
        await context.UserProgresses.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<UserProgress>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.UserProgresses.AsNoTracking().ToListAsync(ct);
    }

    public async Task<UserProgress?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.UserProgresses.FirstOrDefaultAsync(up => up.Id == id, ct);
    }

    public async Task<UserProgress?> GetByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken ct = default)
    {
        return await context.UserProgresses.FirstOrDefaultAsync(up => up.UserId == userId && up.ModuleId == moduleId, ct);
    }

    public async Task<IReadOnlyList<UserProgress>> GetByUserAndTrackAsync(Guid userId, Guid trackId, CancellationToken ct = default)
    {
        return await context.UserProgresses
            .Where(up => up.UserId == userId && up.Module.TrackId == trackId)
            .OrderBy(up => up.Module.Order)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
