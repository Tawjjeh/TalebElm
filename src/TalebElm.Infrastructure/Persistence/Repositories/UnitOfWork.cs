using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Persistence.Repositories;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public IUserRepository Users { get; } 

    public ITrackRepository Tracks { get; } 

    public IModuleRepository Modules { get; }

    public ILessonRepository Lessons { get; }

    public IUserProgressRepository UserProgresses { get; }
    public ILessonResourceRepository LessonResources { get; }

    public IExamRepository Exams { get; }
    public ITrackEnrollmentRepository TrackEnrollments { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Tracks = new TrackRepository(_context);
        Modules = new ModuleRepository(_context);
        Lessons = new LessonRepository(_context);
        Exams = new ExamRepository(_context);
        UserProgresses = new UserProgressRepository(_context);
        TrackEnrollments = new TrackEnrollmentRepository(_context);
        LessonResources = new LessonResourceRepository(_context);

    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}