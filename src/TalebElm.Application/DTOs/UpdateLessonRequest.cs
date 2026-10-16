namespace TalebElm.Application.DTOs;

public record UpdateLessonRequest(
    string Title,
    Guid ModuleId,
    int Order);