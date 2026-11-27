namespace TalebElm.Application.DTOs;

/// <summary>
/// A learner-safe exam question: prompt and points, without the answer key.
/// </summary>
/// <param name="Id">The unique identifier of the question.</param>
/// <param name="Prompt">The question text.</param>
/// <param name="Order">The display order of the question inside the exam.</param>
/// <param name="Points">The points awarded for a correct answer.</param>
/// <param name="Options">The answer options (no <c>IsCorrect</c>).</param>
public record ExamQuestionResponse(
    Guid Id,
    string Prompt,
    int Order,
    int Points,
    IReadOnlyList<ExamOptionResponse> Options);
