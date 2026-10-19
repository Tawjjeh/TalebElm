using System;

namespace TalebElm.Domain.Entities;

public class TrackEnrollment:BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TrackId { get; set; }
    public DateTimeOffset EnrolledAt { get; set; }

    public User User { get; set; } = null!;
    public Track Track { get; set; } = null!;

}
