namespace TalebElm.Domain.Entities;

public class GitHubTask : BaseEntity
{
    public string TemplateRepository { get; set; } = string.Empty;
    public string WorkflowFileName { get; set; } = "test.yml";
    public int? MinPassingTests { get; set; }
    public bool IsActive { get; set; } = true;
}
