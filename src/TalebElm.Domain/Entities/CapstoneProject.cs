namespace TalebElm.Domain.Entities;

public class CapstoneProject : BaseEntity
{
    public Guid TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StarterTemplateRepo { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    
    // Navigation properties
    public Track Track { get; set; } = null!;
    public GitHubTask Task { get; set; } = null!;
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();


    public CapstoneProject() { }
}
