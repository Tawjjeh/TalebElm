namespace TalebElm.Application.DTOs
{
    public record CreateExamRequest(Guid ModuleId, string Title, int PassThreshold);
}
