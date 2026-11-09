using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
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

    [Fact]
    public async Task GetPagedAsync_ReturnsPublishedTracksOnlyWithStableOrderingAndPagination()
    {
        var baseTime = DateTimeOffset.UtcNow;

        var t1 = new Track { Id = Guid.NewGuid(), Name = "T1", Description = "D1", Status = TrackStatus.Published, CreatedAt = baseTime.AddMinutes(-30) };
        var t2 = new Track { Id = Guid.NewGuid(), Name = "T2", Description = "D2", Status = TrackStatus.Draft, CreatedAt = baseTime.AddMinutes(-20) };
        var t3 = new Track { Id = Guid.NewGuid(), Name = "T3", Description = "D3", Status = TrackStatus.Published, CreatedAt = baseTime.AddMinutes(-10) };
        var t4 = new Track { Id = Guid.NewGuid(), Name = "T4", Description = "D4", Status = TrackStatus.Published, CreatedAt = baseTime.AddMinutes(0) };
        var t5 = new Track { Id = Guid.NewGuid(), Name = "T5", Description = "D5", Status = TrackStatus.Archived, CreatedAt = baseTime.AddMinutes(10) };

        await _repository.AddAsync(t1);
        await _repository.AddAsync(t2);
        await _repository.AddAsync(t3);
        await _repository.AddAsync(t4);
        await _repository.AddAsync(t5);
        await SaveChangesAsync();
        ClearTracker();

        // Act - Page 1 with pageSize 2 (published only)
        var (page1Items, totalCount) = await _repository.GetPagedAsync(page: 1, pageSize: 2, publishedOnly: true);

        // Assert - Only 3 published tracks total (t1, t3, t4)
        Assert.Equal(3, totalCount);
        Assert.Equal(2, page1Items.Count);
        Assert.Equal(t1.Id, page1Items[0].Id);
        Assert.Equal(t3.Id, page1Items[1].Id);

        // Act - Page 2 with pageSize 2
        var (page2Items, _) = await _repository.GetPagedAsync(page: 2, pageSize: 2, publishedOnly: true);

        Assert.Single(page2Items);
        Assert.Equal(t4.Id, page2Items[0].Id);
    }

    [Fact]
    public async Task GetPagedAsync_WhenPublishedOnlyIsFalse_ReturnsAllTracks()
    {
        var t1 = new Track { Id = Guid.NewGuid(), Name = "T1", Description = "D1", Status = TrackStatus.Published };
        var t2 = new Track { Id = Guid.NewGuid(), Name = "T2", Description = "D2", Status = TrackStatus.Draft };
        var t3 = new Track { Id = Guid.NewGuid(), Name = "T3", Description = "D3", Status = TrackStatus.Archived };

        await _repository.AddAsync(t1);
        await _repository.AddAsync(t2);
        await _repository.AddAsync(t3);
        await SaveChangesAsync();
        ClearTracker();

        var (items, totalCount) = await _repository.GetPagedAsync(page: 1, pageSize: 10, publishedOnly: false);

        Assert.Equal(3, totalCount);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_WhenTokenIsCancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.GetPagedAsync(page: 1, pageSize: 20, publishedOnly: true, cancellationToken: cts.Token));
    }

    private static Track CreateTrack(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = $"{name} description"
    };
}
