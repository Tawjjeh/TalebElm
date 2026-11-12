namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents detailed lesson information, including its resources.
/// </summary>
/// <param name="Id">The unique identifier of the lesson.</param>
/// <param name="ModuleId">The identifier of the parent module.</param>
/// <param name="Title">The lesson title.</param>
/// <param name="Content">The lesson content.</param>
/// <param name="Order">The lesson's position within its module.</param>
/// <param name="Resources">The resources associated with the lesson.</param>
public record LessonDetailResponse(
    Guid Id,
    Guid ModuleId,
    string Title,
    string Content,
    int Order,
    IReadOnlyList<LessonResourceResponse> Resources);
