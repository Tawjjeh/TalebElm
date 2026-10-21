using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class TrackRepositoryTests : SqliteTestBase
{
    private readonly TrackRepository _repository;

    public TrackRepositoryTests()
    {
        _repository = new TrackRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersistUntilUnitOfWorkSaves()
    {
        var track = CreateTrack("Backend with .NET");

        await _repository.AddAsync(track);

        await using var secondContext = CreateContext();
        Assert.Null(await secondContext.Tracks.FindAsync(track.Id));

        await SaveChangesAsync();
        Assert.NotNull(await secondContext.Tracks.FindAsync(track.Id));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsTrack()
    {
        var track = CreateTrack("Backend with .NET");
        await _repository.AddAsync(track);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByIdAsync(track.Id);

        Assert.NotNull(result);
        Assert.Equal(track.Id, result.Id);
        Assert.Equal("Backend with .NET", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredTracks()
    {
        await _repository.AddAsync(CreateTrack("Backend"));
        await _repository.AddAsync(CreateTrack("Frontend"));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, track => track.Name == "Backend");
        Assert.Contains(result, track => track.Name == "Frontend");
    }

    private static Track CreateTrack(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = $"{name} description"
    };
}
