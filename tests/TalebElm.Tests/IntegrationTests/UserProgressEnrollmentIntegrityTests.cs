using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserProgressEnrollmentIntegrityTests : SqliteTestBase
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ProgressRejectsMissingEnrollmentOrMismatchedTrack(bool enrolledInOtherTrack, bool useOtherTrack)
    {
        var (user, track, module) = await SeedModuleAsync(DbContext);
        var other = new Track { Name = "Second path", Description = "Another test path" };
        DbContext.Tracks.Add(other);
        if (enrolledInOtherTrack)
        {
            DbContext.TrackEnrollments.Add(new TrackEnrollment
            {
                UserId = user.Id,
                TrackId = other.Id
            });
        }
        await SaveChangesAsync();

        DbContext.UserProgresses.Add(new UserProgress
        {
            UserId = user.Id,
            ModuleId = module.Id,
            TrackId = useOtherTrack ? other.Id : track.Id,
            IsUnlocked = true
        });

        var failure = await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
        Assert.Equal(787, Assert.IsType<SqliteException>(failure.InnerException).SqliteExtendedErrorCode);
    }

    [Fact]
    public async Task EnrollCommitsFirstProgressAndRestrictsRemovingItsEnrollment()
    {
        var (user, track, module) = await SeedModuleAsync(DbContext);
        var enrollment = await new TrackEnrollmentService(new UnitOfWork(DbContext)).EnrollAsync(user.Id, track.Id);
        ClearTracker();

        var progress = await DbContext.UserProgresses.SingleAsync();
        Assert.Equal((user.Id, module.Id, track.Id), (progress.UserId, progress.ModuleId, progress.TrackId));
        Assert.True(progress.IsUnlocked);
        Assert.Equal(enrollment.Id, (await DbContext.TrackEnrollments.SingleAsync()).Id);

        var failure = await Assert.ThrowsAsync<SqliteException>(() =>
            DbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM TrackEnrollments WHERE Id = {enrollment.Id}"));
        Assert.Equal(19, failure.SqliteErrorCode);
        Assert.Equal(1, await DbContext.TrackEnrollments.CountAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MigrationPreservesHistoryOrAbortsWhenEnrollmentIsMissing(bool hasEnrollment)
    {
        const string previousMigration = "20261115012058_AddUserProblemCompletion";
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(previousMigration);
        var (user, track, module) = await SeedModuleAsync(db);
        if (hasEnrollment)
        {
            db.TrackEnrollments.Add(new TrackEnrollment { UserId = user.Id, TrackId = track.Id });
            await db.SaveChangesAsync();
        }

        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO UserProgresses (Id, UserId, ModuleId, CreatedAt, IsUnlocked, PassedExam, CompletedRequiredProblems, Score)
            VALUES ({id}, {user.Id}, {module.Id}, {createdAt}, 1, 1, 1, 88)
            """);

        if (hasEnrollment)
        {
            await migrator.MigrateAsync();
            var progress = await db.UserProgresses.AsNoTracking().SingleAsync();
            Assert.Equal((id, createdAt, user.Id, module.Id, track.Id),
                (progress.Id, progress.CreatedAt, progress.UserId, progress.ModuleId, progress.TrackId));
            Assert.True(progress.IsUnlocked && progress.PassedExam && progress.CompletedRequiredProblems);
            Assert.Equal(88, progress.Score);
            await migrator.MigrateAsync(previousMigration);
        }
        else
        {
            var failure = await Assert.ThrowsAsync<SqliteException>(() => migrator.MigrateAsync());
            Assert.Contains("CK_UserProgresses_MatchingEnrollment", failure.Message);
        }

        Assert.Equal(88, await db.Database.SqlQuery<int>($"SELECT Score AS Value FROM UserProgresses WHERE Id = {id}").SingleAsync());
        Assert.Equal(0, await db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM pragma_table_info('UserProgresses') WHERE name = 'TrackId'").SingleAsync());
        Assert.Equal(previousMigration, (await db.Database.GetAppliedMigrationsAsync()).Last());
    }

    private static async Task<(User User, Track Track, Module Module)> SeedModuleAsync(AppDbContext db)
    {
        var user = new User { Name = "Learner", Email = "progress@example.invalid", PasswordHash = "test-hash" };
        var track = new Track { Name = "Test path", Description = "Test path", Status = TrackStatus.Published };
        var module = new Module { Title = "First Module", TrackId = track.Id, Order = 1 };
        db.AddRange(user, track, module);
        await db.SaveChangesAsync();
        return (user, track, module);
    }
}
