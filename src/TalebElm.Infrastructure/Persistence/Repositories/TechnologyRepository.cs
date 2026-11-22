using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class TechnologyRepository(AppDbContext context) : ITechnologyRepository
{
    public async Task<Technology?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Technologies.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IReadOnlyList<Technology>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Technologies.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Technology entity, CancellationToken ct = default)
    {
        await context.Technologies.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Technology>> GetByParentIdAsync(Guid? parentId, CancellationToken ct = default)
    {
        return await context.Technologies
        .Where(t => t.ParentId == parentId)
        .OrderBy(t => t.Order)
        .AsNoTracking()
        .ToListAsync(ct);

    }

    public async Task<Technology?> GetBySlugPathAsync(IReadOnlyList<string> slugs, CancellationToken ct = default)
    {
        Guid? parentId = null;
        Technology? current = null;

        foreach (var slug in slugs)
        {
            current = await context.Technologies
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ParentId == parentId && t.Slug == slug, ct);

            if (current is null)
                return null;

            parentId = current.Id;
        }

        return current;

    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetChildCountsAsync(
        IReadOnlyCollection<Guid> parentIds,
        CancellationToken ct = default)
    {
        if (parentIds.Count == 0)
            return new Dictionary<Guid, int>();

        var counts = await context.Technologies
            .AsNoTracking()
            .Where(t => t.ParentId != null && parentIds.Contains(t.ParentId.Value))
            .GroupBy(t => t.ParentId)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts
            .Where(x => x.ParentId is not null)
            .ToDictionary(x => x.ParentId!.Value, x => x.Count);
    }
}
