using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TechnologyKind Kind { get; set; }
    public int Order { get; set; }
    public Guid? ParentId { get; set; }
    public Technology? Parent { get; set; }
    public ICollection<Technology> Children { get; set; } = new List<Technology>();
}
