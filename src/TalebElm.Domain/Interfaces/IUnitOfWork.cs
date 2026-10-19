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
    Task<int> SaveChangesAsync();
}
