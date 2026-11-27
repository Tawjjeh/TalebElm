using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction
{
    public interface ITrackService
    {
        Task<IReadOnlyList<TrackResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TrackResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
        Task<PagedTracksResponse> GetPagedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
        Task<TrackDetailResponse> GetByIdAsync(Guid trackId, CancellationToken cancellationToken = default);
    }
}
