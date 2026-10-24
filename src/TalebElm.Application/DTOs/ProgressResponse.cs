namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents a user's progress in a module.
    /// </summary>
    /// <param name="ModuleId">The unique identifier of the module.</param>
    /// <param name="IsUnlocked">Indicates whether the module is unlocked for the user.</param>
    /// <param name="PassedExam">Indicates whether the user has passed the module exam.</param>
    /// <param name="Score">The score achieved by the user in the exam.</param>
    public record ProgressResponse(Guid ModuleId, bool IsUnlocked, bool PassedExam, int Score);
}