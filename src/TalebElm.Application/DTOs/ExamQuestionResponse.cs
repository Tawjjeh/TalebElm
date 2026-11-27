namespace TalebElm.Application.DTOs;

/// <summary>
/// Learner-safe representation of one exam question. Never carries IsCorrect.
/// </summary>
/// <param name="Id">The unique identifier of the question.</param>
/// <param name="Prompt">The question text.</param>
/// <param name="Order">The display order within the exam.</param>
/// <param name="Points">The points awarded for a correct answer.</param>
/// <param name="Options">The learner-visible options (id and text only).</param>
public record ExamQuestionResponse(
    Guid Id,
    string Prompt,
    int Order,
    int Points,
    IReadOnlyList<ExamOptionResponse> Options);
