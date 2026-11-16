using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Domain.Enums;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class ResourceRepository(AppDbContext context) : IResourceRepository
{
    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Resources.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Resources.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Resource entity, CancellationToken ct = default)
    {
        await context.Resources.AddAsync(entity, ct);
    }

    public async Task<(IReadOnlyList<Resource> Items, int TotalCount)> GetApprovedPagedAsync(
        Guid? technologyId,
        string? searchQuery,
        string? language,
        ResourceLevel? level,
        bool? isPaid,
        ResourceKind? kind,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Resources
            .AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Approved);

        if (technologyId is not null)
            query = query.Where(r => r.TechnologyId == technologyId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
            query = query.Where(r => r.Title.Contains(searchQuery));

        if (language is not null)
            query = query.Where(r => r.Language == language);

        if (level is not null)
            query = query.Where(r => r.Level == level);

        if (isPaid is not null)
            query = query.Where(r => r.IsPaid == isPaid);

        if (kind is not null)
            query = query.Where(r => r.Kind == kind);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.Title)
            .ThenBy(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Resource>> GetForHealthCheckAsync(CancellationToken ct = default)
    {
        return await context.Resources
            .AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Approved)
            .ToListAsync(ct);
    }
}
