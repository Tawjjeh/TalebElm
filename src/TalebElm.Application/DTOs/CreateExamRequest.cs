namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents the data required to create a new exam.
    /// </summary>
    /// <param name="ModuleId">The unique identifier of the module associated with the exam.</param>
    /// <param name="Title">The title of the exam.</param>
    /// <param name="PassThreshold">The minimum score required to pass the exam.</param>
    /// <param name="Questions">The exam questions (at least 1, each with at least 2 options and exactly one correct).</param>
    public record CreateExamRequest(
        Guid ModuleId,
        string Title,
        int PassThreshold,
        IReadOnlyList<CreateExamQuestionRequest> Questions);
}
