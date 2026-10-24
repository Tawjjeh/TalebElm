namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the response data for an exam.
/// </summary>
/// <param name="Id">The unique identifier of the exam.</param>
/// <param name="Title">The title of the exam.</param>
/// <param name="PassThreshold">The minimum score required to pass the exam.</param>
/// <param name="ModuleId">The unique identifier of the module associated with the exam.</param>
public record ExamResponse(Guid Id, string Title, int PassThreshold, Guid ModuleId);