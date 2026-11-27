using Microsoft.EntityFrameworkCore;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Application.Validators;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

/// <summary>
/// Track/Module admin writes through the real services + UnitOfWork against
/// SQLite: order compaction on create/move/delete and the 409 delete guards.
/// </summary>
public class TrackModuleAdminSqliteTests : SqliteTestBase
{
    private TrackService TrackService(UnitOfWork uow)
        => new(uow, new CreateTrackRequestValidator(), new UpdateTrackRequestValidator());

    private ModuleService ModuleService(UnitOfWork uow)
        => new(uow, new CreateModuleRequestValidator(), new UpdateModuleRequestValidator());

    private async Task<(Guid TrackId, List<Guid> ModuleIds)> SeedTrackWithModulesAsync(int count, TrackStatus status = TrackStatus.Draft)
    {
        var track = new Track { Name = "T", Description = "d", Status = status };
        DbContext.Tracks.Add(track);
        var moduleIds = new List<Guid>();
        for (var i = 1; i <= count; i++)
        {
            var module = new Module { TrackId = track.Id, Title = $"M{i}", Summary = "s", Order = i };
            DbContext.Modules.Add(module);
            moduleIds.Add(module.Id);
        }
        await SaveChangesAsync();
        ClearTracker();
        return (track.Id, moduleIds);
    }

    [Fact]
    public async Task CreateModuleAtOrder2_ShiftsSiblingsAndPersists()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(3);
        var uow = new UnitOfWork(DbContext);

        var created = await ModuleService(uow).CreateAsync(
            new CreateModuleRequest("New", "s", 2, trackId));

        Assert.Equal(2, created.Order);
        await using var second = CreateContext();
        Assert.Equal(["M1", "New", "M2", "M3"],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync());
        Assert.Equal([1, 2, 3, 4],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Order).ToListAsync());
    }

    [Fact]
    public async Task UpdateModuleOrder_MovesWithinTrack()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(3);
        var uow = new UnitOfWork(DbContext);

        var updated = await ModuleService(uow).UpdateAsync(moduleIds[0],
            new UpdateModuleRequest("M1", "s", 3, trackId));

        Assert.Equal(3, updated.Order);
        await using var second = CreateContext();
        Assert.Equal(["M2", "M3", "M1"],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync());
    }

    [Fact]
    public async Task DeleteModuleInMiddle_CompactsRemainingSiblings()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(3);
        var uow = new UnitOfWork(DbContext);

        await ModuleService(uow).DeleteAsync(moduleIds[1]);

        await using var second = CreateContext();
        Assert.Equal(["M1", "M3"],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Title).ToListAsync());
        Assert.Equal([1, 2],
            await second.Modules.Where(m => m.TrackId == trackId)
                .OrderBy(m => m.Order).Select(m => m.Order).ToListAsync());
    }

    [Fact]
    public async Task UpdateTrack_Draft_UpdatesFields()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(1);
        var uow = new UnitOfWork(DbContext);

        var updated = await TrackService(uow).UpdateAsync(trackId, new UpdateTrackRequest("Renamed", "New desc"));

        Assert.Equal("Renamed", updated.Name);
        await using var second = CreateContext();
        Assert.Equal("Renamed", (await second.Tracks.FindAsync(trackId))!.Name);
    }

    [Fact]
    public async Task UpdateTrack_Published_ThrowsValidation()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(1, TrackStatus.Published);
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ValidationException>(() =>
            TrackService(uow).UpdateAsync(trackId, new UpdateTrackRequest("Renamed", "New desc")));
    }

    [Fact]
    public async Task DeleteTrack_WithModules_ThrowsConflictAndKeepsTrack()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(1);
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => TrackService(uow).DeleteAsync(trackId));

        await using var second = CreateContext();
        Assert.NotNull(await second.Tracks.FindAsync(trackId));
    }

    [Fact]
    public async Task DeleteTrack_Empty_Deletes()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(0);
        var uow = new UnitOfWork(DbContext);

        await TrackService(uow).DeleteAsync(trackId);

        await using var second = CreateContext();
        Assert.Null(await second.Tracks.FindAsync(trackId));
    }

    [Fact]
    public async Task DeleteTrack_WithEnrollment_ThrowsConflict()
    {
        var (trackId, _) = await SeedTrackWithModulesAsync(0);
        var user = new User { Name = "Learner", Email = "learner@example.com", PasswordHash = "hash" };
        DbContext.Users.Add(user);
        DbContext.TrackEnrollments.Add(new TrackEnrollment { UserId = user.Id, TrackId = trackId });
        await SaveChangesAsync();
        ClearTracker();
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => TrackService(uow).DeleteAsync(trackId));
    }

    [Fact]
    public async Task DeleteModule_WithLesson_ThrowsConflictAndKeepsModule()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(1);
        DbContext.Lessons.Add(new Lesson { ModuleId = moduleIds[0], Title = "L1", Content = "c", Order = 1 });
        await SaveChangesAsync();
        ClearTracker();
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => ModuleService(uow).DeleteAsync(moduleIds[0]));

        await using var second = CreateContext();
        Assert.NotNull(await second.Modules.FindAsync(moduleIds[0]));
    }

    [Fact]
    public async Task DeleteModule_WithExam_ThrowsConflict()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(1);
        DbContext.Exams.Add(new Exam { ModuleId = moduleIds[0], Title = "E", PassThreshold = 60 });
        await SaveChangesAsync();
        ClearTracker();
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => ModuleService(uow).DeleteAsync(moduleIds[0]));
    }

    [Fact]
    public async Task DeleteModule_WithProgress_ThrowsConflict()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(1);
        var user = new User { Name = "Learner", Email = "learner@example.com", PasswordHash = "hash" };
        DbContext.Users.Add(user);
        DbContext.UserProgresses.Add(new UserProgress { UserId = user.Id, ModuleId = moduleIds[0] });
        await SaveChangesAsync();
        ClearTracker();
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => ModuleService(uow).DeleteAsync(moduleIds[0]));
    }

    [Fact]
    public async Task DeleteTrack_WithProgress_ThrowsConflict()
    {
        var (trackId, moduleIds) = await SeedTrackWithModulesAsync(1);
        var user = new User { Name = "Learner", Email = "learner@example.com", PasswordHash = "hash" };
        DbContext.Users.Add(user);
        DbContext.UserProgresses.Add(new UserProgress { UserId = user.Id, ModuleId = moduleIds[0] });
        await SaveChangesAsync();
        ClearTracker();
        var uow = new UnitOfWork(DbContext);

        await Assert.ThrowsAsync<ConflictException>(() => TrackService(uow).DeleteAsync(trackId));
    }
}
