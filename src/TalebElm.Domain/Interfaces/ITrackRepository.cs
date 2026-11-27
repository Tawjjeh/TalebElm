using TalebElm.Domain.Entities;
namespace TalebElm.Domain.Interfaces;

public interface ITrackRepository : IRepository<Track>
{
    Task<(IReadOnlyList<Track> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool publishedOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a track for deletion. Callers run the #251 conflict guards
    /// first, so no dependent row may exist when this is called.
    /// </summary>
    Task RemoveAsync(Track track, CancellationToken ct = default);
}
