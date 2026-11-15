using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class UserProblemCompletionTest
{
    [Fact]
    public void Model_ShouldAllowOneCompletionPerUserAndProblem()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(UserProblemCompletion));

        Assert.NotNull(entityType);
        Assert.Equal("UserProblemCompletions", entityType.GetTableName());
        Assert.Equal(
            200,
            entityType.FindProperty(nameof(UserProblemCompletion.ExternalSubmissionId))!.GetMaxLength());
        Assert.Equal(
            40,
            entityType.FindProperty(nameof(UserProblemCompletion.VerifiedHeadCommitSha))!.GetMaxLength());

        var index = entityType.GetIndexes().Single(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(UserProblemCompletion.UserId),
                nameof(UserProblemCompletion.ModuleProblemId)
            }));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Model_ShouldRestrictDeletingReferencedRows()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(UserProblemCompletion));

        Assert.NotNull(entityType);

        var foreignKeys = entityType.GetForeignKeys().ToList();

        Assert.Equal(2, foreignKeys.Count);
        Assert.All(foreignKeys, foreignKey =>
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
    }
}
