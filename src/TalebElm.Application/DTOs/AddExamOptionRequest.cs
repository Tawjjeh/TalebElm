namespace TalebElm.Application.DTOs;

/// <summary>
/// The data used to add one option to a question.
/// </summary>
/// <param name="Text">The option text. Required.</param>
/// <param name="IsCorrect">Whether this option is the correct answer. After the add, the question must still have exactly one correct option (Decision 7).</param>
public record AddExamOptionRequest(string Text, bool IsCorrect);
