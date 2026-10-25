using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class Resource : BaseEntity
{
    public Guid? TechnologyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceKind Kind { get; set; }
    public string? Url { get; set; }
    public string? Citation { get; set; }
    public string Language { get; set; } = string.Empty;
    public ResourceLevel Level { get; set; }
    public bool IsPaid { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Approved;
    public int ConsecutiveFailureCount { get; set; }
    public DateTimeOffset? LastCheckedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public Technology? Technology { get; set; }
    public User? CreatedByUser { get; set; }
}
