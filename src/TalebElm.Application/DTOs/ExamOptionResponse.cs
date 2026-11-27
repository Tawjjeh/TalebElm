namespace TalebElm.Application.DTOs;

/// <summary>
/// A learner-safe exam option: identifier and text only. Never carries <c>IsCorrect</c>.
/// </summary>
/// <param name="Id">The unique identifier of the option.</param>
/// <param name="Text">The option text.</param>
public record ExamOptionResponse(Guid Id, string Text);
