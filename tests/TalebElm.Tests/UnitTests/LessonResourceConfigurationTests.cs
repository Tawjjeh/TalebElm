using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class LessonResourceConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigureRequiredLessonRelationshipWithRestrict()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(LessonResource));
        var foreignKey = entityType?.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Lesson));

        Assert.NotNull(foreignKey);
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(nameof(LessonResource.LessonId), foreignKey.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public async Task DeleteLesson_WithAssociatedResources_ThrowsDbUpdateException()
    {
        var (lesson, _) = await CreateLessonWithResourceAsync();

        var lessonToDelete = await DbContext.Lessons.FindAsync(lesson.Id);
        Assert.NotNull(lessonToDelete);
        DbContext.Lessons.Remove(lessonToDelete);

        await Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync());
    }

    [Fact]
    public async Task GetByLessonIdAsync_ReturnsOnlyThatLessonOrderedByOrder()
    {
        var (lesson, resource) = await CreateLessonWithResourceAsync();
        var first = new LessonResource
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            Title = "First",
            Kind = LessonResourceKind.Article,
            Order = 1
        };
        var second = new LessonResource
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            Title = "Second",
            Kind = LessonResourceKind.Video,
            Order = 2
        };
        var otherLesson = new Lesson
        {
            Id = Guid.NewGuid(),
            Title = "Other",
            Content = "Other content",
            Order = 2,
            ModuleId = lesson.ModuleId
        };
        var otherResource = new LessonResource
        {
            Id = Guid.NewGuid(),
            LessonId = otherLesson.Id,
            Title = "Other resource",
            Kind = LessonResourceKind.Book,
            Order = 1
        };

        DbContext.Lessons.Add(otherLesson);
        DbContext.LessonResources.AddRange(first, second, otherResource);
        await SaveChangesAsync();
        ClearTracker();

        var repository = new LessonResourceRepository(DbContext);
        var result = await repository.GetByLessonIdAsync(lesson.Id);

        Assert.Equal(new[] { first.Id, second.Id, resource.Id }, result.Select(r => r.Id));
    }

    private async Task<(Lesson Lesson, LessonResource Resource)> CreateLessonWithResourceAsync()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), Title = "C#", Order = 1, TrackId = track.Id };
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            Title = "Variables",
            Content = "Content",
            Order = 1,
            ModuleId = module.Id
        };
        var resource = new LessonResource
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            Title = "Docs",
            Kind = LessonResourceKind.Documentation,
            Url = "https://example.com/docs",
            Order = 3
        };

        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Lessons.Add(lesson);
        DbContext.LessonResources.Add(resource);
        await SaveChangesAsync();
        ClearTracker();

        return (lesson, resource);
    }
}
