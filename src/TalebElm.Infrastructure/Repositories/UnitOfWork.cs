using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IUserProgressRepository? _userProgressRepository;

    public IUserRepository Users => throw new NotImplementedException();

    public ITrackRepository Tracks => throw new NotImplementedException();

    public IModuleRepository Modules => throw new NotImplementedException();
    public IExamRepository Exams => throw new NotImplementedException();

    public IUserProgressRepository UserProgresses => _userProgressRepository ??= new UserProgressRepository(context);

    public Task<int> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}