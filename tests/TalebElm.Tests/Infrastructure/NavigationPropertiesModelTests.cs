using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.Infrastructure;

public class NavigationPropertiesModelTests : SqliteTestBase
{
    [Fact]
    public void Track_Modules_Navigation_ShouldBeMapped_WithRestrictDelete()
    {
        var moduleType = DbContext.Model.FindEntityType(typeof(Module));
        Assert.NotNull(moduleType);

        var fk = Assert.Single(
            moduleType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(Module.TrackId))
                && fk.PrincipalEntityType.ClrType == typeof(Track));

        Assert.Equal(nameof(Module.TrackId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(Track.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);

        Assert.Equal(nameof(Module.Track), fk.DependentToPrincipal?.Name);
        Assert.Equal(nameof(Track.Modules), fk.PrincipalToDependent?.Name);
    }

    [Fact]
    public void Lesson_Module_Navigation_ShouldBeMapped_WithRestrictDelete()
    {
        var lessonType = DbContext.Model.FindEntityType(typeof(Lesson));
        Assert.NotNull(lessonType);

        var fk = Assert.Single(
            lessonType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(Lesson.ModuleId))
                && fk.PrincipalEntityType.ClrType == typeof(Module));

        Assert.Equal(nameof(Lesson.ModuleId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(Module.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);

        Assert.Equal(nameof(Lesson.Module), fk.DependentToPrincipal?.Name);
        Assert.Equal(nameof(Module.Lessons), fk.PrincipalToDependent?.Name);
    }

    [Fact]
    public void Exam_Module_Navigation_ShouldBeOneToOne_WithRestrictDelete_AndUniqueIndex()
    {
        var examType = DbContext.Model.FindEntityType(typeof(Exam));
        Assert.NotNull(examType);

        var fk = Assert.Single(
            examType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(Exam.ModuleId))
                && fk.PrincipalEntityType.ClrType == typeof(Module));

        Assert.Equal(nameof(Exam.ModuleId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(Module.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.True(fk.IsUnique);

        Assert.Equal(nameof(Exam.Module), fk.DependentToPrincipal?.Name);
        Assert.Equal(nameof(Module.Exam), fk.PrincipalToDependent?.Name);

        var uniqueIndex = examType!.GetIndexes()
            .SingleOrDefault(x => x.Properties.Select(p => p.Name)
                .SequenceEqual(new[] { nameof(Exam.ModuleId) }));
        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex!.IsUnique);
    }

    [Fact]
    public void UserProgress_User_Navigation_ShouldBeMapped_WithRestrictDelete()
    {
        var progressType = DbContext.Model.FindEntityType(typeof(UserProgress));
        Assert.NotNull(progressType);

        var fk = Assert.Single(
            progressType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(UserProgress.UserId))
                && fk.PrincipalEntityType.ClrType == typeof(User));

        Assert.Equal(nameof(UserProgress.UserId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(User.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.Equal(nameof(UserProgress.User), fk.DependentToPrincipal?.Name);
    }

    [Fact]
    public void UserProgress_Module_Navigation_ShouldBeMapped_WithRestrictDelete()
    {
        var progressType = DbContext.Model.FindEntityType(typeof(UserProgress));
        Assert.NotNull(progressType);

        var fk = Assert.Single(
            progressType!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(UserProgress.ModuleId))
                && fk.PrincipalEntityType.ClrType == typeof(Module));

        Assert.Equal(nameof(UserProgress.ModuleId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(Module.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.Equal(nameof(UserProgress.Module), fk.DependentToPrincipal?.Name);
    }

    [Fact]
    public void UserProgress_ShouldKeep_UniqueUserModuleIndex()
    {
        var progressType = DbContext.Model.FindEntityType(typeof(UserProgress));
        Assert.NotNull(progressType);

        var index = progressType!.GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name)
                .SequenceEqual(new[]
                {
                    nameof(UserProgress.UserId),
                    nameof(UserProgress.ModuleId)
                }));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public async Task GetByUserAndTrackAsync_ShouldWork_ThroughNavigations()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var otherTrack = new Track { Id = Guid.NewGuid(), Name = "Frontend", Description = "Frontend track" };
        var user = new User { Id = Guid.NewGuid(), Name = "Learner", Email = "learner@test.com", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.AddRange(track, otherTrack);
        DbContext.Users.Add(user);

        var m1 = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M1", Order = 2 };
        var m2 = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M2", Order = 1 };
        var other = new Module { Id = Guid.NewGuid(), TrackId = otherTrack.Id, Title = "Other", Order = 1 };
        DbContext.Modules.AddRange(m1, m2, other);
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = user.Id, ModuleId = m1.Id },
            new UserProgress { UserId = user.Id, ModuleId = m2.Id },
            new UserProgress { UserId = user.Id, ModuleId = other.Id });
        await SaveChangesAsync();
        ClearTracker();

        var result = await new TalebElm.Infrastructure.Repositories.UserProgressRepository(DbContext)
            .GetByUserAndTrackAsync(user.Id, track.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(m2.Id, result[0].ModuleId);
        Assert.Equal(m1.Id, result[1].ModuleId);
    }
}
