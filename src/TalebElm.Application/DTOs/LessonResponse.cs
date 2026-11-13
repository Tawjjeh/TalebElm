namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents basic information about a lesson.
    /// </summary>
    /// <param name="Id">The unique identifier of the lesson.</param>
    /// <param name="Title">The title of the lesson.</param>
    /// <param name="ModuleId">The identifier of the module containing the lesson.</param>
    public record LessonResponse(Guid Id, string Title, Guid ModuleId);
}
