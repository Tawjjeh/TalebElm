using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ITrackEnrollmentRepository : IRepository<TrackEnrollment>
{
    Task<bool> ExistsAsync(Guid userId, Guid trackId, CancellationToken ct = default);

    /// <summary>Whether any learner is enrolled in the track.</summary>
    Task<bool> ExistsForTrackAsync(Guid trackId, CancellationToken ct = default);
}
