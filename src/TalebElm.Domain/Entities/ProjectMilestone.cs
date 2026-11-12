namespace TalebElm.Domain.Entities;

public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Order { get; set; }
    public string WorkflowJobName { get; set; } = string.Empty;

    public CapstoneProject Project { get; set; } = null!;
    public Module? Module { get; set; }
}
