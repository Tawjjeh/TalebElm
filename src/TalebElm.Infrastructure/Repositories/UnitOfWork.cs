using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users => throw new NotImplementedException();

    public ITrackRepository Tracks => throw new NotImplementedException();

    public IModuleRepository Modules => throw new NotImplementedException();

    public Task<int> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}
