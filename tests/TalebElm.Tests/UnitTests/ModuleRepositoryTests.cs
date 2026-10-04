using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Repositories;

namespace TalebElm.Tests.UnitTests;

public class ModuleRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetByTrackIdAsync_EmptyTrack_ReturnsEmptyList()
    {
        await using var context = CreateContext();
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        var result = await new ModuleRepository(context).GetByTrackIdAsync(track.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTrackIdAsync_MultipleModules_ReturnsOrderedByOrder()
    {
        await using var context = CreateContext();
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        context.Tracks.Add(track);
        context.Modules.AddRange(
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Third", Order = 3 },
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "First", Order = 1 },
            new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "Second", Order = 2 });
        await context.SaveChangesAsync();

        var result = await new ModuleRepository(context).GetByTrackIdAsync(track.Id);

        Assert.Equal([1, 2, 3], result.Select(m => m.Order));
    }

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNull()
    {
        await using var context = CreateContext();

        var result = await new ModuleRepository(context).GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}