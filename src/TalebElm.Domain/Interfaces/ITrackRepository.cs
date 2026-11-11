using TalebElm.Domain.Entities;
namespace TalebElm.Domain.Interfaces;

public interface ITrackRepository : IRepository<Track>
{
    Task<(IReadOnlyList<Track> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool publishedOnly = true,
        CancellationToken cancellationToken = default);
}
