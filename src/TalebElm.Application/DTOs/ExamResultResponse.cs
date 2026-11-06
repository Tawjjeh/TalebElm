namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the result of a user's exam attempt.
/// </summary>
/// <param name="ExamId">The unique identifier of the exam.</param>
/// <param name="Passed">Indicates whether the user passed the exam.</param>
/// <param name="Score">The score achieved by the user.</param>
public record ExamResultResponse(Guid ExamId, bool Passed, int Score);