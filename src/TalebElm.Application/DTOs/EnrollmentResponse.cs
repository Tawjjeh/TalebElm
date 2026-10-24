namespace TalebElm.Application.DTOs;

public record EnrollmentResponse(Guid Id, Guid TrackId, DateTimeOffset EnrolledAt);
