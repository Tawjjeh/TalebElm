namespace TalebElm.Application.DTOs
{
    public record ProgressResponse(
        Guid ModuleId,
        bool IsUnlocked,
        bool PassedExam,
        int Score,
        int RequiredProblemsTotal = 0,
        int RequiredProblemsCompleted = 0);
}
