using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class ProjectMilestoneTrackMigrationTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public async Task UpgradeBackfillsValidMilestoneOrRejectsCrossTrackLink(bool linkModule, bool sameTrack)
    {
        const string previousMigration = "20261115012058_AddUserProblemCompletion";
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(previousMigration);

        var track = new Track { Name = "First", Description = "Capstone track" };
        var otherTrack = new Track { Name = "Second", Description = "Other track" };
        var project = new CapstoneProject
        {
            TrackId = track.Id, Title = "Capstone", Description = "Test project",
            Task = new GitHubTask { TemplateRepository = "example/template" }
        };
        var module = new Module { TrackId = sameTrack ? track.Id : otherTrack.Id, Title = "Module", Order = 1 };
        db.AddRange(track, otherTrack, project, module);
        await db.SaveChangesAsync();

        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        Guid? moduleId = linkModule ? module.Id : null;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO ProjectMilestones (Id, CreatedAt, ProjectId, ModuleId, Title, Instructions, "Order", WorkflowJobName)
            VALUES ({id}, {createdAt}, {project.Id}, {moduleId}, 'Test milestone', 'Write tests', 1, 'test')
            """);

        if (linkModule && !sameTrack)
        {
            var failure = await Assert.ThrowsAsync<SqliteException>(() => migrator.MigrateAsync());
            Assert.Contains("CK_ProjectMilestones_MatchingTrack", failure.Message);
        }
        else
        {
            await migrator.MigrateAsync();
            var milestone = await db.ProjectMilestones.AsNoTracking().SingleAsync();
            Assert.Equal((id, createdAt, project.Id, track.Id, moduleId),
                (milestone.Id, milestone.CreatedAt, milestone.ProjectId, milestone.TrackId, milestone.ModuleId));
            Assert.Equal(("Test milestone", "Write tests", 1, "test"),
                (milestone.Title, milestone.Instructions, milestone.Order, milestone.WorkflowJobName));

            var failure = await Assert.ThrowsAsync<SqliteException>(() => db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE ProjectMilestones SET TrackId = {otherTrack.Id}, ModuleId = NULL WHERE Id = {id}"));
            Assert.Equal(787, failure.SqliteExtendedErrorCode);
            await migrator.MigrateAsync(previousMigration);
        }

        Assert.Equal(previousMigration, (await db.Database.GetAppliedMigrationsAsync()).Last());
        var restoredModuleId = await db.Database.SqlQuery<Guid?>(
            $"SELECT ModuleId AS Value FROM ProjectMilestones WHERE Id = {id}").SingleAsync();
        Assert.Equal(moduleId, restoredModuleId);
        Assert.Equal(0, await db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM pragma_table_info('ProjectMilestones') WHERE name = 'TrackId'").SingleAsync());
    }
}
