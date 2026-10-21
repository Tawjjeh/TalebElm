using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class Track : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrackStatus Status { get; set; } = TrackStatus.Draft;
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<TrackEnrollment> TrackEnrollments { get; set; } = new List<TrackEnrollment>();
}
