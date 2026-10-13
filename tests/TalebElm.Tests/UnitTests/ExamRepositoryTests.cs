using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class ExamRepositoryTests : SqliteTestBase
{
    private readonly ExamRepository _repository;

    public ExamRepositoryTests()
    {
        _repository = new ExamRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersistUntilUnitOfWorkSaves()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Module 1", Order = 1 };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        await SaveChangesAsync();
        ClearTracker();

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            ModuleId = module.Id,
            Title = "Module 1 Exam",
            PassThreshold = 70
        };

        await _repository.AddAsync(exam);

        await using var secondContext = CreateContext();
        Assert.Null(await secondContext.Exams.FindAsync(exam.Id));

        await SaveChangesAsync();
        Assert.NotNull(await secondContext.Exams.FindAsync(exam.Id));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamExists_ReturnsExam()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Module 1", Order = 1 };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        var exam = new Exam { Id = Guid.NewGuid(), ModuleId = module.Id, Title = "Module 1 Exam", PassThreshold = 70 };
        DbContext.Exams.Add(exam);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByIdAsync(exam.Id);

        Assert.NotNull(result);
        Assert.Equal(exam.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsStoredExams()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module1 = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "First Module", Order = 1 };
        var module2 = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Second Module", Order = 2 };
        DbContext.Tracks.Add(track);
        DbContext.Modules.AddRange(module1, module2);
        var exams = new[]
        {
            new Exam { Id = Guid.NewGuid(), ModuleId = module1.Id, Title = "First", PassThreshold = 60 },
            new Exam { Id = Guid.NewGuid(), ModuleId = module2.Id, Title = "Second", PassThreshold = 70 }
        };
        DbContext.Exams.AddRange(exams);
        await SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(exams.Select(exam => exam.Id).Order(), result.Select(exam => exam.Id).Order());
    }

    [Fact]
    public async Task GetByModuleIdAsync_ReturnsOnlyTheRequestedModuleExam()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var requestedModule = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Requested", Order = 1 };
        var otherModule = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Other", Order = 2 };
        DbContext.Tracks.Add(track);
        DbContext.Modules.AddRange(requestedModule, otherModule);
        var requestedModuleId = requestedModule.Id;
        var otherModuleId = otherModule.Id;
        var requestedExam = new Exam { Id = Guid.NewGuid(), ModuleId = requestedModuleId, Title = "Requested", PassThreshold = 70 };
        DbContext.Exams.AddRange(
            requestedExam,
            new Exam { Id = Guid.NewGuid(), ModuleId = otherModuleId, Title = "Other", PassThreshold = 60 });
        await SaveChangesAsync();

        var result = await _repository.GetByModuleIdAsync(requestedModuleId);

        Assert.NotNull(result);
        Assert.Equal(requestedExam.Id, result.Id);
    }

    [Fact]
    public async Task GetByModuleIdAsync_WhenModuleHasNoExam_ReturnsNull()
    {
        var result = await _repository.GetByModuleIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}