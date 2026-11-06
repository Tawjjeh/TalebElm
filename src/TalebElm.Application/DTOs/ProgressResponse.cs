namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents a learner's progress in a module.
    /// </summary>
    /// <param name="ModuleId">The unique identifier of the module.</param>
    /// <param name="IsUnlocked">Whether the module is unlocked for the learner.</param>
    /// <param name="PassedExam">Whether the learner has passed the module exam.</param>
    /// <param name="Score">The latest exam score for the module.</param>
    /// <param name="RequiredProblemsTotal">
    /// The number of required problems in the module. This is a Target value and is always
    /// <c>0</c> until the counts are computed (issue #413); it must not be used to derive
    /// unlock or enrollment state.
    /// </param>
    /// <param name="RequiredProblemsCompleted">
    /// The number of completed required problems. This is a Target value and is always
    /// <c>0</c> until the counts are computed (issue #413). Do not treat
    /// <c>RequiredProblemsCompleted == RequiredProblemsTotal</c> as the unlock gate: the gate
    /// is the stored sticky <c>UserProgress.CompletedRequiredProblems</c> flag (Decision 32),
    /// because adding a required problem later must never re-lock a learner.
    /// </param>
    public record ProgressResponse(
        Guid ModuleId,
        bool IsUnlocked,
        bool PassedExam,
        int Score,
        int RequiredProblemsTotal = 0,
        int RequiredProblemsCompleted = 0);
}
