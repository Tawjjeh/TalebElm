namespace TalebElm.Domain.Entities;

public class ProjectMilestone
{
    public Guid CapstoneProjectId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string WorkflowJobName { get; set; } = string.Empty;

    // Navigation property
    public CapstoneProject CapstoneProject { get; set; } = null!;
}
