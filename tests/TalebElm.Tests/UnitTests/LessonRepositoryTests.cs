using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class LessonRepositoryTests : SqliteTestBase
{
    private readonly LessonRepository _repository;

    public LessonRepositoryTests() => _repository = new LessonRepository(DbContext);

    [Fact]
    public async Task GetByModuleIdAsync_ReturnsOnlyRequestedModuleLessons_InOrder()
    {
        var track = new Track { Name = "Backend", Description = "Backend track" };
        var module = new Module { Title = "C#", Order = 1, TrackId = track.Id };
        var otherModule = new Module { Title = "SQL", Order = 2, TrackId = track.Id };
        var secondLesson = new Lesson { Title = "Second", Content = "Content", Order = 2, ModuleId = module.Id };
        var firstLesson = new Lesson { Title = "First", Content = "Content", Order = 1, ModuleId = module.Id };
        var otherLesson = new Lesson { Title = "Other", Content = "Content", Order = 1, ModuleId = otherModule.Id };

        DbContext.Tracks.Add(track);
        DbContext.Modules.AddRange(module, otherModule);
        DbContext.Lessons.AddRange(secondLesson, firstLesson, otherLesson);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByModuleIdAsync(module.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(firstLesson.Id, result[0].Id);
        Assert.Equal(secondLesson.Id, result[1].Id);
    }

    [Fact]
    public async Task GetByModuleIdAsync_WhenModuleHasNoLessons_ReturnsEmptyList()
    {
        var result = await _repository.GetByModuleIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonIsMissing_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSave()
    {
        var track = new Track { Name = "Backend", Description = "Backend track" };
        var module = new Module { Title = "C#", Order = 1, TrackId = track.Id };
        var lesson = new Lesson { Title = "First", Content = "Content", Order = 1, ModuleId = module.Id };

        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        await SaveChangesAsync();

        await _repository.AddAsync(lesson);

        await using (var before = CreateContext())
            Assert.Equal(0, before.Lessons.Count(entry => entry.Id == lesson.Id));

        await SaveChangesAsync();

        await using (var after = CreateContext())
            Assert.Equal(1, after.Lessons.Count(entry => entry.Id == lesson.Id));
    }
}
