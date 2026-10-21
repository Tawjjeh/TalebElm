using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class LessonResource : BaseEntity
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonResourceKind Kind { get; set; }
    public string? Url { get; set; }
    public string? Citation { get; set; }
    public int Order { get; set; }
    public Lesson Lesson { get; set; } = null!;
}
