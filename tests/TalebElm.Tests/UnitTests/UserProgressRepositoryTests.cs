using System;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class UserProgressRepositoryTests : SqliteTestBase
{
    private readonly UserProgressRepository _repository;
    public UserProgressRepositoryTests() => _repository = new UserProgressRepository(DbContext);

    [Fact]
    public async Task GetByUserAndModuleAsync_WhenMissing_ReturnsNull()
    {
        var result = await _repository.GetByUserAndModuleAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserAndTrackAsync_ShouldIsolateUsers()
    {
        var trackId = Guid.NewGuid();
        var module = new Module { Id = Guid.NewGuid(), TrackId = trackId, Order = 1, Title = "Module 1" };
        await DbContext.Modules.AddAsync(module);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        await _repository.AddAsync(new UserProgress { UserId = userA, ModuleId = module.Id, Score = 100 });
        await _repository.AddAsync(new UserProgress { UserId = userB, ModuleId = module.Id, Score = 50 });
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByUserAndTrackAsync(userA, trackId);

        Assert.Single(result);
        Assert.Equal(userA, result[0].UserId);
        Assert.Equal(100, result[0].Score);
    }

    [Fact]
    public async Task GetByUserAndTrackAsync_ShouldOrderByModuleOrderAscending()
    {
        var trackId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var m1 = new Module { Id = Guid.NewGuid(), TrackId = trackId, Order = 1, Title = "M1" };
        var m2 = new Module { Id = Guid.NewGuid(), TrackId = trackId, Order = 2, Title = "M2" };
        var m3 = new Module { Id = Guid.NewGuid(), TrackId = trackId, Order = 3, Title = "M3" };
        await DbContext.Modules.AddRangeAsync(m1, m2, m3);

        await _repository.AddAsync(new UserProgress { UserId = userId, ModuleId = m3.Id });
        await _repository.AddAsync(new UserProgress { UserId = userId, ModuleId = m1.Id });
        await _repository.AddAsync(new UserProgress { UserId = userId, ModuleId = m2.Id });
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByUserAndTrackAsync(userId, trackId);

        Assert.Equal(3, result.Count);
        Assert.Equal(m1.Id, result[0].ModuleId);
        Assert.Equal(m2.Id, result[1].ModuleId);
        Assert.Equal(m3.Id, result[2].ModuleId);
    }

    [Fact]
    public async Task Update_ShouldPersistChanges_WhenEntityIsTracked()
    {
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var progress = new UserProgress { UserId = userId, ModuleId = moduleId, Score = 40, PassedExam = false };

        await _repository.AddAsync(progress);
        await DbContext.SaveChangesAsync();

        var tracked = await _repository.GetByUserAndModuleAsync(userId, moduleId);
        Assert.NotNull(tracked);

        tracked.Score = 95;
        tracked.PassedExam = true;
        await DbContext.SaveChangesAsync();

        var updated = await _repository.GetByUserAndModuleAsync(userId, moduleId);
        Assert.NotNull(updated);
        Assert.Equal(95, updated.Score);
        Assert.True(updated.PassedExam);
    }
}
