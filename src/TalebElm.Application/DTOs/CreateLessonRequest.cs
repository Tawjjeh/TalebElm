using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

public record CreateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order,
    string Content,
    LessonType Type);