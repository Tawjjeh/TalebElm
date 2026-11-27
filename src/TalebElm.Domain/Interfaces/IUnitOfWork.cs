namespace TalebElm.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    IModuleRepository Modules { get; }
    ILessonRepository Lessons { get; }
    IExamRepository Exams { get; }
    IUserProgressRepository UserProgresses { get; }
    ITrackEnrollmentRepository TrackEnrollments { get; }
    ILessonResourceRepository LessonResources { get; }
    ITechnologyRepository Technologies { get; }
    IResourceRepository Resources { get; }
    IUserExternalAccountRepository UserExternalAccounts { get; }
    IModuleProblemRepository ModuleProblems { get; }
    IUserProblemCompletionRepository UserProblemCompletions { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="action"/> inside a single database transaction,
    /// committing when it completes and rolling back when it throws.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
