namespace TalebElm.Application.DTOs;

/// <summary>
/// An answer option to create as part of a new exam question.
/// </summary>
/// <param name="Text">The option text.</param>
/// <param name="IsCorrect">Whether this is the single correct option of its question.</param>
public record CreateExamOptionRequest(string Text, bool IsCorrect);
