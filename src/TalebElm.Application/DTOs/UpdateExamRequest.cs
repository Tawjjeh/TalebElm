namespace TalebElm.Application.DTOs;

/// <summary>
/// The data used to update an existing exam.
/// </summary>
/// <param name="Title">The new exam title. Required, at most 150 characters.</param>
/// <param name="PassThreshold">The new minimum score required to pass, 1–100. Changing it does not alter existing UserProgress rows.</param>
public record UpdateExamRequest(string Title, int PassThreshold);
