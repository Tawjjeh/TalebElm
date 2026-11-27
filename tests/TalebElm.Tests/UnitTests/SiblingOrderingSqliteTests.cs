using Microsoft.EntityFrameworkCore;
using TalebElm.Application.Ordering;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

/// <summary>
/// <see cref="SiblingOrdering"/> against real SQLite: the unique
/// <c>(TrackId, Order)</c> / <c>(ModuleId, Order)</c> indexes and the
/// <c>Order &gt; 0</c> check constraints are enforced per statement, so these
/// tests prove the shifts never trip them — for both Modules and Lessons.
/// </summary>
public class SiblingOrderingSqliteTests : SqliteTestBase
{
    private async Task<Guid> SeedTrackWithModulesAsync(int count)
    {
        var track = new Track { Name = "T", Description = "t", Status = TrackStatus.Published };
        DbContext.Tracks.Add(track);
        for (var i = 1; i <= count; i++)
            DbContext.Modules.Add(new Module { TrackId = track.Id, Title = $"M{i}", Order = i });
        await SaveChangesAsync();
        ClearTracker();
        return track.Id;
    }

    private async Task<Guid> SeedModuleWithLessonsAsync(int count)
    {
        var track = new Track { Name = "T", Description = "t", Status = TrackStatus.Published };
        var module = new Module { TrackId = track.Id, Title = "M1", Order = 1 };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        for (var i = 1; i <= count; i++)
            DbContext.Lessons.Add(new Lesson { ModuleId = module.Id, Title = $"L{i}", Order = i, Content = "c" });
        await SaveChangesAsync();
        ClearTracker();
        return module.Id;
    }

    [Fact]
    public async Task InsertModuleAtMiddle_KeepsContiguousOrder()
    {
        var trackId = await SeedTrackWithModulesAsync(5);
        var uow = new UnitOfWork(DbContext);

        var modules = (await uow.Modules.GetByTrackIdForUpdateAsync(trackId)).OrderBy(m => m.Order).ToList();
        SiblingOrdering.PrepareInsert(modules, 2);
        await uow.Modules.AddAsync(new Module { TrackId = trackId, Title = "New", Order = 2 });
        await uow.SaveChangesAsync();

        await using var second = CreateContext();
        var titles = await second.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync();
        Assert.Equal(["M1", "New", "M2", "M3", "M4", "M5"], titles);
        Assert.Equal([1, 2, 3, 4, 5, 6],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Order).ToListAsync());
    }

    [Fact]
    public async Task InsertLessonAtStart_KeepsContiguousOrder()
    {
        var moduleId = await SeedModuleWithLessonsAsync(3);
        var uow = new UnitOfWork(DbContext);

        var lessons = (await uow.Lessons.GetByModuleIdForUpdateAsync(moduleId)).OrderBy(l => l.Order).ToList();
        SiblingOrdering.PrepareInsert(lessons, 1);
        await uow.Lessons.AddAsync(new Lesson { ModuleId = moduleId, Title = "New", Order = 1, Content = "c" });
        await uow.SaveChangesAsync();

        await using var second = CreateContext();
        var titles = await second.Lessons.Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.Order).Select(l => l.Title).ToListAsync();
        Assert.Equal(["New", "L1", "L2", "L3"], titles);
    }

    [Fact]
    public async Task DeleteModuleInMiddle_CompactsRemainingSiblings()
    {
        var trackId = await SeedTrackWithModulesAsync(5);
        var uow = new UnitOfWork(DbContext);

        var modules = (await uow.Modules.GetByTrackIdForUpdateAsync(trackId)).OrderBy(m => m.Order).ToList();
        var removed = modules.Single(m => m.Order == 3);
        DbContext.Modules.Remove(removed);
        modules.Remove(removed);
        SiblingOrdering.PrepareDelete(modules, 3);
        await uow.SaveChangesAsync();

        await using var second = CreateContext();
        var titles = await second.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync();
        Assert.Equal(["M1", "M2", "M4", "M5"], titles);
        Assert.Equal([1, 2, 3, 4],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Order).ToListAsync());
    }

    [Fact]
    public async Task DeleteLastLesson_ShiftsNothing()
    {
        var moduleId = await SeedModuleWithLessonsAsync(3);
        var uow = new UnitOfWork(DbContext);

        var lessons = (await uow.Lessons.GetByModuleIdForUpdateAsync(moduleId)).OrderBy(l => l.Order).ToList();
        var removedLesson = lessons.Single(l => l.Order == 3);
        DbContext.Lessons.Remove(removedLesson);
        lessons.Remove(removedLesson);
        SiblingOrdering.PrepareDelete(lessons, 3);
        await uow.SaveChangesAsync();

        await using var second = CreateContext();
        Assert.Equal(["L1", "L2"],
            await second.Lessons.Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.Order).Select(l => l.Title).ToListAsync());
    }

    [Fact]
    public async Task MoveModuleDown_PlacesMoverAndKeepsContiguousOrder()
    {
        var trackId = await SeedTrackWithModulesAsync(5);
        var uow = new UnitOfWork(DbContext);

        var modules = (await uow.Modules.GetByTrackIdForUpdateAsync(trackId)).OrderBy(m => m.Order).ToList();
        await SiblingOrdering.MoveAsync(uow, modules, modules.Single(m => m.Order == 2), 5);

        await using var second = CreateContext();
        var titles = await second.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync();
        Assert.Equal(["M1", "M3", "M4", "M5", "M2"], titles);
        Assert.Equal([1, 2, 3, 4, 5],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Order).ToListAsync());
    }

    [Fact]
    public async Task MoveLessonUp_PlacesMoverAndKeepsContiguousOrder()
    {
        var moduleId = await SeedModuleWithLessonsAsync(5);
        var uow = new UnitOfWork(DbContext);

        var lessons = (await uow.Lessons.GetByModuleIdForUpdateAsync(moduleId)).OrderBy(l => l.Order).ToList();
        await SiblingOrdering.MoveAsync(uow, lessons, lessons.Single(l => l.Order == 5), 2);

        await using var second = CreateContext();
        var titles = await second.Lessons.Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.Order).Select(l => l.Title).ToListAsync();
        Assert.Equal(["L1", "L5", "L2", "L3", "L4"], titles);
    }

    [Fact]
    public async Task NaiveSingleSaveMove_ThrowsCircularDependency()
    {
        // Documents why MoveAsync is two-phase: a mover and its intermediate
        // range form a dependency cycle that EF Core cannot order in one save.
        var trackId = await SeedTrackWithModulesAsync(5);

        var modules = await DbContext.Modules.Where(m => m.TrackId == trackId)
            .OrderBy(m => m.Order).ToListAsync();
        var moving = modules.Single(m => m.Order == 2);
        moving.Order = 5;
        foreach (var m in modules.Where(m => m.Order > 2 && m.Order <= 5 && m != moving))
            m.Order -= 1;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => SaveChangesAsync());
        Assert.Contains("circular dependency", ex.Message);
    }
}
