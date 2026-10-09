using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context ;
    
    public IUserRepository Users { get; } 

    public ITrackRepository Tracks { get; } 

    public IModuleRepository Modules { get; }

    public IExamRepository Exams { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Tracks = new TrackRepository(_context);
        Modules = new ModuleRepository(_context);
        Exams = new ExamRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}