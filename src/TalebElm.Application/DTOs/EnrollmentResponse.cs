namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the enrollment information for a learner in a track.
/// </summary>
/// <param name="Id">The unique identifier of the enrollment.</param>
/// <param name="TrackId">The identifier of the enrolled track.</param>
/// <param name="EnrolledAt">The date and time when the enrollment was created.</param>
public record EnrollmentResponse(Guid Id, Guid TrackId, DateTimeOffset EnrolledAt);
