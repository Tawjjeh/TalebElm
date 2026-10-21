using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

public record LessonResourceResponse(
    Guid Id,
    string Title,
    LessonResourceKind Kind,
    string? Url,
    string? Citation,
    int Order);
