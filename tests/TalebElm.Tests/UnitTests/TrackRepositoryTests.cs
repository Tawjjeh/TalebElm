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
        var track = new Track
        {
            Id = Guid.NewGuid(),
            Name = "Backend with .NET",
            Description = "Backend development using .NET"
        };

        await _repository.AddAsync(track);

        await using var secondContext = CreateContext();
        Assert.Null(await secondContext.Tracks.FindAsync(track.Id));

        await SaveChangesAsync();
        Assert.NotNull(await secondContext.Tracks.FindAsync(track.Id));
    }
}