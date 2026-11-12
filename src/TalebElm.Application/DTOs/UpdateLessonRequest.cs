namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the data required to update a lesson.
/// </summary>
/// <param name="Title">The updated lesson title.</param>
/// <param name="ModuleId">The identifier of the parent module.</param>
/// <param name="Order">The updated lesson position within its module.</param>
public record UpdateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order);