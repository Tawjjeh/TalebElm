using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;
public class ModuleProblem : BaseEntity
{
    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public ProblemKind Kind { get; set; }

    public string ExternalIdentifier { get; set; } = string.Empty;
    public Guid? GitHubTaskId { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public int? Rating { get; set; }

}

