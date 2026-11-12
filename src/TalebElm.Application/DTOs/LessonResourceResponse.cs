using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents a learning resource associated with a lesson.
/// </summary>
/// <param name="Id">The unique identifier of the resource.</param>
/// <param name="Title">The resource title.</param>
/// <param name="Kind">The type of learning resource.</param>
/// <param name="Url">The optional resource URL.</param>
/// <param name="Citation">The optional resource citation.</param>
/// <param name="Order">The display order of the resource.</param>
public record LessonResourceResponse(
    Guid Id,
    string Title,
    LessonResourceKind Kind,
    string? Url,
    string? Citation,
    int Order);
