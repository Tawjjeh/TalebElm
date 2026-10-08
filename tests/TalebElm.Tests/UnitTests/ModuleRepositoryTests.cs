using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class ModuleRepositoryTests : SqliteTestBase
{
    private readonly ModuleRepository _repository;

    public ModuleRepositoryTests()
    {
        _repository = new ModuleRepository(DbContext);
    }

    [Fact]
    public async Task GetByTrackIdAsync_EmptyTrack_ReturnsEmptyList()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        DbContext.Tracks.Add(track);
        await SaveChangesAsync();

        var result = await _repository.GetByTrackIdAsync(track.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTrackIdAsync_MultipleModules_ReturnsOrderedByOrder()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var otherTrack = new Track { Id = Guid.NewGuid(), Name = "Frontend", Description = "Frontend track" };
        DbContext.Tracks.AddRange(track, otherTrack);
        DbContext.Modules.AddRange(
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Third", Order = 3 },
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "First", Order = 1 },
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Second", Order = 2 },
            new Module { Id = Guid.NewGuid(), TrackId = otherTrack.Id, Title = "Other track module", Order = 1 });
        await SaveChangesAsync();

        var result = await _repository.GetByTrackIdAsync(track.Id);

        Assert.Equal(3, result.Count);
        Assert.Equal([1, 2, 3], result.Select(m => m.Order));
        Assert.All(result, module => Assert.Equal(track.Id, module.TrackId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleExists_ReturnsModule()
    {
        var module = new Module { Id = Guid.NewGuid(), Title = "C# basics", Order = 1 };
        DbContext.Modules.Add(module);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByIdAsync(module.Id);

        Assert.NotNull(result);
        Assert.Equal(module.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllModules()
    {
        var modules = new[]
        {
            new Module { Id = Guid.NewGuid(), Title = "First", Order = 1 },
            new Module { Id = Guid.NewGuid(), Title = "Second", Order = 2 }
        };
        DbContext.Modules.AddRange(modules);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(modules.Select(module => module.Id).Order(), result.Select(module => module.Id).Order());
    }

    [Fact]
    public async Task AddAsync_DoesNotPersistUntilUnitOfWorkSaves()
    {
        var module = new Module { Id = Guid.NewGuid(), Title = "C# basics" };
        await _repository.AddAsync(module);

        await using var secondContext = CreateContext();
        Assert.Null(await secondContext.Modules.FindAsync(module.Id));

        await SaveChangesAsync();
        Assert.NotNull(await secondContext.Modules.FindAsync(module.Id));
    }
}