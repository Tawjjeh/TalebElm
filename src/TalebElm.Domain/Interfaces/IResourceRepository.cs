using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Interfaces;

public interface IResourceRepository : IRepository<Resource>
{
    Task<(IReadOnlyList<Resource> Items, int TotalCount)> GetApprovedPagedAsync(
         Guid? technologyId,
         string? searchQuery,
         string? language,
         ResourceLevel? level,
         bool? isPaid,
         ResourceKind? kind,
         int pageNumber,
         int pageSize,
         CancellationToken ct = default);

    Task<IReadOnlyList<Resource>> GetForHealthCheckAsync(CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> GetApprovedResourceCountsAsync(
        IReadOnlyCollection<Guid> technologyIds,
        CancellationToken ct = default);
}
