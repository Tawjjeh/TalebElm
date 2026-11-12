using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the data required to create a lesson.
/// </summary>
/// <param name="Title">The title of the new lesson.</param>
/// <param name="ModuleId">The identifier of the parent module.</param>
/// <param name="Order">The lesson's position within the module.</param>
/// <param name="Content">The lesson body content.</param>
/// <param name="Type">The format type of the lesson.</param>
public record CreateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order,
    string Content,
    LessonType Type);