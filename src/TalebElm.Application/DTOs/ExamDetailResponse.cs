namespace TalebElm.Application.DTOs;

/// <summary>
/// Learner-safe view of an exam: questions and options without the answer key.
/// <see cref="ExamOptionResponse"/> never carries <c>IsCorrect</c>.
/// </summary>
/// <param name="Id">The unique identifier of the exam.</param>
/// <param name="Title">The title of the exam.</param>
/// <param name="PassThreshold">The minimum score required to pass the exam.</param>
/// <param name="ModuleId">The unique identifier of the module associated with the exam.</param>
/// <param name="Questions">The exam questions, ordered by <c>Order</c>.</param>
public record ExamDetailResponse(
    Guid Id,
    string Title,
    int PassThreshold,
    Guid ModuleId,
    IReadOnlyList<ExamQuestionResponse> Questions);
