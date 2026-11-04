namespace TalebElm.Domain.Entities;

public class CapstoneProject
{
    public Guid TrackId { get; set; }
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public Track Track { get; set; } = null!;
    // GitHubTask entity is not created yet, it will be created in a separated issue.
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
}
