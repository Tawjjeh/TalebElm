using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Repositories;

namespace TalebElm.Tests.UnitTests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsTrackAndModule_InOneCommit()
    {
        // In-memory SQLite lives only as long as the connection stays open
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var track = new Track
        {
            Id = Guid.NewGuid(),
            Name = "Backend with .NET",
            Description = "Backend development track using .NET",
            Status = TrackStatus.Published
        };

        var module = new Module
        {
            Id = Guid.NewGuid(),
            Title = "Introduction to C#",
            Summary = "Basic syntax and core concepts",
            Order = 1,
            TrackId = track.Id
        };

        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var uow = new UnitOfWork(context);

            // Both adds go through the same shared context
            await uow.Tracks.AddAsync(track);
            await uow.Modules.AddAsync(module);

            // AddAsync must not persist anything on its own
            await using (var check = new AppDbContext(options))
            {
                Assert.Empty(check.Set<Track>());
                Assert.Empty(check.Set<Module>());
            }

            // A single commit for both repositories
            await uow.SaveChangesAsync();
        }

        // A fresh context proves the data really reached the database
        await using (var verify = new AppDbContext(options))
        {
            Assert.NotNull(await verify.Set<Track>().FindAsync(track.Id));
            Assert.NotNull(await verify.Set<Module>().FindAsync(module.Id));
        }
    }
}