using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task AddAsync(Track entity, CancellationToken ct = default)
    {
        await context.Tracks.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Track>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Tracks.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Track?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Tracks.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Track> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool publishedOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tracks.AsNoTracking();

        if (publishedOnly)
            query = query.Where(t => t.Status == TrackStatus.Published);

        var totalCount = await query.CountAsync(cancellationToken);

        var offset = (long)(page - 1) * pageSize;
        if (offset >= totalCount || offset > int.MaxValue)
            return ([], totalCount);

        var items = await query
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }
}
