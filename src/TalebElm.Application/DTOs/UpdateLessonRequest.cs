using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

public record UpdateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order,
    string Content,
    LessonType Type);