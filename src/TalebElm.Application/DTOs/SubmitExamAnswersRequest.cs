namespace TalebElm.Application.DTOs;

public record SubmitExamAnswersRequest(Guid ExamId, IReadOnlyList<SubmittedAnswer> Answers);
