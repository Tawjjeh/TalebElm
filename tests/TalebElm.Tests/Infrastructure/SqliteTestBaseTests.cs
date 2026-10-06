using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;

namespace TalebElm.Tests.Infrastructure;

public class SqliteTestBaseTests : SqliteTestBase
{
    [Fact]
    public async Task SqliteTestBase_ProvidesWorkingRelationalContext()
    {
        Assert.True(DbContext.Database.IsSqlite());
        Assert.True(await DbContext.Database.CanConnectAsync());

        var connection = DbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys;";
        var foreignKeysEnabled = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, foreignKeysEnabled);

        var track = new Track { Id = Guid.NewGuid(), Name = "Smoke", Description = "Smoke test" };
        DbContext.Tracks.Add(track);
        await SaveChangesAsync();
        ClearTracker();

        Assert.True(await DbContext.Tracks.AnyAsync(t => t.Id == track.Id));
    }
}
