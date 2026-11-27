namespace TalebElm.Application.DTOs;

/// <summary>
/// Learner-safe representation of one exam option. Never carries IsCorrect.
/// </summary>
/// <param name="Id">The unique identifier of the option.</param>
/// <param name="Text">The option text.</param>
public record ExamOptionResponse(Guid Id, string Text);
