namespace TalebElm.Application.DTOs;

/// <summary>
/// A question to create as part of a new exam.
/// </summary>
/// <param name="Prompt">The question text.</param>
/// <param name="Order">The display order of the question inside the exam (unique per exam, starting at 1).</param>
/// <param name="Points">The points awarded for a correct answer (must be greater than 0).</param>
/// <param name="Options">The answer options (at least 2, exactly one correct).</param>
public record CreateExamQuestionRequest(
    string Prompt,
    int Order,
    int Points,
    IReadOnlyList<CreateExamOptionRequest> Options);
