namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents the data required to create a new exam.
    /// </summary>
    /// <param name="ModuleId">The unique identifier of the module associated with the exam.</param>
    /// <param name="Title">The title of the exam.</param>
    /// <param name="PassThreshold">The minimum score required to pass the exam.</param>
    public record CreateExamRequest(Guid ModuleId, string Title, int PassThreshold);
}