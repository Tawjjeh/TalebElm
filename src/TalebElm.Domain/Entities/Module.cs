namespace TalebElm.Domain.Entities;

public class Module : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid TrackId { get; set; }
}
