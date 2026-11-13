using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class UserProblemCompletionTest
{
    private static async Task<(SqliteConnection Connection, AppDbContext Context)>
        CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return (connection, context);
    }

    [Fact]
    public async Task UserProblemCompletion_ShouldHaveUniqueUserIdAndModuleProblemIdIndex()
    {
        var (connection, context) = await CreateContextAsync();

        try
        {
            //. This line will 
            var entityType = context.Model.FindEntityType(
                typeof(UserProblemCompletion));

            Assert.NotNull(entityType);

            //.Using the index to search for the duplication
            //. and Select the name column 
            var index = entityType!.GetIndexes()
                .SingleOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                        .SequenceEqual(new[]
                        {
                            nameof(UserProblemCompletion.UserId),
                            nameof(UserProblemCompletion.ModuleProblemId)
                        }));

            Assert.NotNull(index);
            Assert.True(index.IsUnique);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UserProblemCompletion_ForeignKeys_ShouldUseRestrictDelete()
    {
        var (connection, context) = await CreateContextAsync();

        try
        {
            //. getting the model
            var entityType = context.Model.FindEntityType(
                typeof(UserProblemCompletion));

            //. verifying if the return value is not null
            Assert.NotNull(entityType);

            var userForeignKey = entityType!.GetForeignKeys()
                .SingleOrDefault(fk =>
                    fk.Properties.Select(p => p.Name)
                        .SequenceEqual(new[] { nameof(UserProblemCompletion.UserId) }));

            var problemForeignKey = entityType.GetForeignKeys()
                .SingleOrDefault(fk =>
                    fk.Properties.Select(p => p.Name)
                        .SequenceEqual(new[] { nameof(UserProblemCompletion.ModuleProblemId) }));

            Assert.NotNull(userForeignKey);
            Assert.Equal(DeleteBehavior.Restrict, userForeignKey!.DeleteBehavior);

            Assert.NotNull(problemForeignKey);
            Assert.Equal(DeleteBehavior.Restrict, problemForeignKey!.DeleteBehavior);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
