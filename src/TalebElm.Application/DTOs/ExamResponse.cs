namespace TalebElm.Application.DTOs;

public record ExamResponse(Guid Id, string Title, int PassThreshold, Guid ModuleId)
{}
