namespace TalebElm.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    IModuleRepository Modules { get; }
    IExamRepository Exams { get; }
    IUserProgressRepository UserProgresses { get; }
    Task<int> SaveChangesAsync();
}
