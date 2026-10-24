namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the data required to submit an exam result.
/// </summary>
/// <param name="ExamId">The unique identifier of the exam being submitted.</param>
/// <param name="Score">The score achieved by the user in the exam.</param>
public record SubmitExamRequest(Guid ExamId, int Score);