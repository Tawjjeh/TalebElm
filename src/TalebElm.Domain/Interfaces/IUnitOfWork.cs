namespace TalebElm.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    IModuleRepository Modules { get; }
    Task<int> SaveChangesAsync();
}
