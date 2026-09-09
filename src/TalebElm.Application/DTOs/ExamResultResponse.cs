namespace TalebElm.Application.DTOs;

public record ExamResultResponse(Guid ExamId, bool Passed, int Score);
