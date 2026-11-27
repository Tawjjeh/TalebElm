using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserProblemCompletionIntegrityTests : SqliteTestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletionRequiresProgressForItsProblemsModule(bool hasUnrelatedProgress)
    {
        var (user, problem) = await SeedProblemAsync(DbContext);
        if (hasUnrelatedProgress)
        {
            var otherModule = await AddOtherModuleAsync(problem.ModuleId);
            DbContext.UserProgresses.Add(new UserProgress { UserId = user.Id, ModuleId = otherModule.Id });
            await SaveChangesAsync();
        }

        DbContext.UserProblemCompletions.Add(new UserProblemCompletion(user.Id, problem, DateTime.UtcNow));

        var failure = await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
        Assert.Equal(787, Assert.IsType<SqliteException>(failure.InnerException).SqliteExtendedErrorCode);
    }

    [Fact]
    public async Task MatchingProgressAllowsCompletionAndRestrictsDeletes()
    {
        var (user, problem) = await SeedProblemAsync(DbContext);
        var otherModule = await AddOtherModuleAsync(problem.ModuleId);
        var progress = new UserProgress { UserId = user.Id, ModuleId = problem.ModuleId, IsUnlocked = true };
        DbContext.UserProgresses.AddRange(progress, new UserProgress { UserId = user.Id, ModuleId = otherModule.Id });
        var completion = new UserProblemCompletion(user.Id, problem, DateTime.UtcNow, externalSubmissionId: "synthetic-accepted");
        DbContext.UserProblemCompletions.Add(completion);

        await SaveChangesAsync();
        ClearTracker();

        Assert.Equal(problem.ModuleId, (await DbContext.UserProblemCompletions.SingleAsync()).ModuleId);
        var mismatch = await Assert.ThrowsAsync<SqliteException>(() => DbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE UserProblemCompletions SET ModuleId = {otherModule.Id} WHERE Id = {completion.Id}"));
        Assert.Equal(787, mismatch.SqliteExtendedErrorCode);

        var deleteProgress = await Assert.ThrowsAsync<SqliteException>(() =>
            DbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM UserProgresses WHERE Id = {progress.Id}"));
        var deleteProblem = await Assert.ThrowsAsync<SqliteException>(() =>
            DbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ModuleProblem WHERE Id = {problem.Id}"));
        Assert.Equal(19, deleteProgress.SqliteErrorCode);
        Assert.Equal(19, deleteProblem.SqliteErrorCode);

        DbContext.UserProblemCompletions.Add(new UserProblemCompletion(user.Id, problem, DateTime.UtcNow));
        var duplicate = await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
        Assert.Equal(2067, Assert.IsType<SqliteException>(duplicate.InnerException).SqliteExtendedErrorCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MigrationPreservesVerificationMetadataOrAbortsWhenProgressIsMissing(bool hasProgress)
    {
        const string previousMigration = "20261115012058_AddUserProblemCompletion";
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(previousMigration);
        var (user, problem) = await SeedProblemAsync(db);
        if (hasProgress)
        {
            db.UserProgresses.Add(new UserProgress { UserId = user.Id, ModuleId = problem.ModuleId });
            await db.SaveChangesAsync();
        }

        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var verifiedAt = DateTime.UtcNow.AddDays(-1);
        var versionId = Guid.NewGuid();
        const string submission = "synthetic-accepted";
        const string details = "{}";
        var sha = new string('a', 40);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO UserProblemCompletions (
                Id, UserId, ModuleProblemId, CreatedAt, VerifiedAtUtc, ExternalSubmissionId, DetailsJson, VerifiedHeadCommitSha, TaskVersionId
            )
            VALUES ({id}, {user.Id}, {problem.Id}, {createdAt}, {verifiedAt}, {submission}, {details}, {sha}, {versionId})
            """);

        if (hasProgress)
        {
            await migrator.MigrateAsync();
            var completion = await db.UserProblemCompletions.AsNoTracking().SingleAsync();
            Assert.Equal((id, createdAt, user.Id, problem.Id, problem.ModuleId),
                (completion.Id, completion.CreatedAt, completion.UserId, completion.ModuleProblemId, completion.ModuleId));
            Assert.Equal((verifiedAt, submission, details, sha, (Guid?)versionId),
                (completion.VerifiedAtUtc, completion.ExternalSubmissionId, completion.DetailsJson,
                    completion.VerifiedHeadCommitSha, completion.TaskVersionId));
            await migrator.MigrateAsync(previousMigration);
        }
        else
        {
            var failure = await Assert.ThrowsAsync<SqliteException>(() => migrator.MigrateAsync());
            Assert.Contains("CK_UserProblemCompletions_MatchingProgress", failure.Message);
        }

        Assert.Equal(submission, await db.Database.SqlQuery<string>(
            $"SELECT ExternalSubmissionId AS Value FROM UserProblemCompletions WHERE Id = {id}").SingleAsync());
        Assert.Equal(0, await db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM pragma_table_info('UserProblemCompletions') WHERE name = 'ModuleId'").SingleAsync());
        Assert.Equal(previousMigration, (await db.Database.GetAppliedMigrationsAsync()).Last());
    }

    private static async Task<(User User, ModuleProblem Problem)> SeedProblemAsync(AppDbContext db)
    {
        var user = new User { Name = "Learner", Email = "completion@example.invalid", PasswordHash = "test-hash" };
        var track = new Track { Name = "Test path", Description = "Test path" };
        var module = new Module { Title = "Practice", TrackId = track.Id, Order = 1 };
        var problem = new ModuleProblem
        {
            ModuleId = module.Id, Title = "Practice problem", Kind = ProblemKind.CodeforcesProblem,
            ExternalIdentifier = "1A", Order = 1
        };
        db.AddRange(user, track, module, problem);
        await db.SaveChangesAsync();
        return (user, problem);
    }

    private async Task<Module> AddOtherModuleAsync(Guid moduleId)
    {
        var module = await DbContext.Modules.SingleAsync(m => m.Id == moduleId);
        var otherModule = new Module { Title = "Other Module", TrackId = module.TrackId, Order = 2 };
        DbContext.Modules.Add(otherModule);
        return otherModule;
    }
}
