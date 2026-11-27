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

        /// <summary>
        /// Updates a track's name and description. Only <c>Draft</c> tracks can
        /// be edited; editing a <c>Published</c> or <c>Archived</c> track throws
        /// a validation error (see #266 for the status lifecycle).
        /// </summary>
        Task<TrackResponse> UpdateAsync(Guid id, UpdateTrackRequest request, CancellationToken ct = default);

        /// <summary>
        /// Deletes a track. Throws a conflict when the track still has Modules,
        /// enrollments, or learner progress.
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
