using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Domain.Entities;

public class Lesson : BaseEntity, IOrderedSibling
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Order { get; set; }

    public Guid ModuleId { get; set; }

    public LessonType Type { get; set; } = LessonType.Text;

    public Module Module { get; set; } = default!;
}
