
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class ProjectMilestoneConfigurationTests
{
    //. Method to return the connection (SQLLite) and the context
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
    public async Task WorkflowJobName_ShouldHaveMaxLength100_AndBeRequired()
    {
        var (connection, context) = await CreateContextAsync();

        try
        {
            var entityType = context.Model.FindEntityType(
                typeof(ProjectMilestone));

            Assert.NotNull(entityType);

            var property = entityType.FindProperty(
                nameof(ProjectMilestone.WorkflowJobName));

            Assert.NotNull(property);
            Assert.Equal(100, property.GetMaxLength());
            Assert.False(property.IsNullable);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ProjectMilestone_ShouldHaveUniqueProjectIdAndOrderIndex()
    {
        var (connection, context) = await CreateContextAsync();

        try
        {
            var entityType = context.Model.FindEntityType(
                typeof(ProjectMilestone));

            var index = entityType!.GetIndexes()
                .SingleOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                        .SequenceEqual(new[]
                        {
                            nameof(ProjectMilestone.ProjectId),
                            nameof(ProjectMilestone.Order)
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
    public async Task ProjectMilestone_ShouldHaveUniqueProjectIdAndWorkflowJobNameIndex()
    {
        var (connection, context) = await CreateContextAsync();

        try
        {
            var entityType = context.Model.FindEntityType(
                typeof(ProjectMilestone));

            var index = entityType!.GetIndexes()
                .SingleOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                        .SequenceEqual(new[]
                        {
                            nameof(ProjectMilestone.ProjectId),
                            nameof(ProjectMilestone.WorkflowJobName)
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
}
