using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class Track : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public TrackStatus Status { get; set; } = default!;
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<TrackEnrollment> TrackEnrollments { get; set; } = new List<TrackEnrollment>();
}
