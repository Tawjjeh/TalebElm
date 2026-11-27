namespace TalebElm.Application.DTOs;

/// <summary>
/// The data used to update one question of an exam.
/// </summary>
/// <param name="Prompt">The new question text. Required.</param>
/// <param name="Order">The new display order within the exam, starting at 1. Must be unique within the exam.</param>
/// <param name="Points">The new points awarded for a correct answer. Must be greater than 0.</param>
public record UpdateExamQuestionRequest(string Prompt, int Order, int Points);
