namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the data required to create a lesson.
/// </summary>
/// <param name="Title">The title of the new lesson.</param>
/// <param name="ModuleId">The identifier of the parent module.</param>
/// <param name="Order">The lesson's position within the module.</param>
public record CreateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order);