namespace TalebElm.Application.DTOs;

public record CreateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order);