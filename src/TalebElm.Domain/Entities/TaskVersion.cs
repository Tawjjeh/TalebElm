using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class TaskVersion : BaseEntity
{
    public Guid GitHubTaskId { get; set; }
    public GitHubTask GitHubTask { get; set; } = default!;
    public string VersionTag { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string WorkflowFileSha { get; set; } = string.Empty;
    public string TestTreeSha { get; set; } = string.Empty;
    public TaskVersionStatus Status { get; set; } = TaskVersionStatus.Active;
}
