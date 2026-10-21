using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Persistence.Repositories;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    
    public IUserRepository Users { get; } 

    public ITrackRepository Tracks { get; } 

    public IModuleRepository Modules { get; }

    public ILessonRepository Lessons { get; }

    public IUserProgressRepository UserProgresses { get; }
    public ILessonResourceRepository LessonResources { get; }

    public IExamRepository Exams { get; }
    public ITrackEnrollmentRepository TrackEnrollments { get; }

    public UnitOfWork(AppDbContext context, ILogger<UnitOfWork>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<UnitOfWork>.Instance;
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
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ConflictMessage(ex) is { } message)
        {
            // A race past an application pre-check: the database's unique index or
            // restrictive foreign key is the backstop. Report a conflict, not a 500,
            // and keep the provider's message (table and column names) out of it.
            _logger.LogWarning(ex, "A database constraint rejected the save; reporting a conflict.");
            throw new ConflictException(message, ex);
        }
    }

    // SQLite extended result codes, https://www.sqlite.org/rescode.html. Any other
    // failure stays a DbUpdateException and so an internal error.
    private static string? ConflictMessage(DbUpdateException exception)
        => (exception.InnerException as SqliteException)?.SqliteExtendedErrorCode switch
        {
            1555 or 2067 => "The change conflicts with an existing record.", // PRIMARYKEY, UNIQUE
            // FOREIGNKEY. EF Core deletes with DELETE ... RETURNING, which SQLite runs as an
            // internal trigger, so a delete blocked by a restrictive key arrives as
            // CONSTRAINT_TRIGGER (1811). The schema defines no triggers of its own.
            787 or 1811 => "The change conflicts with related records.",
            _ => null
        };
}