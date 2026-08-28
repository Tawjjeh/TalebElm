namespace TalebElm.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    Task<int> SaveChangesAsync();
}
