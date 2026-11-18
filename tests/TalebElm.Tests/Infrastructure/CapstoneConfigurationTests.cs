using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class CapstoneConfigurationTests
{
    [Fact]
    public void Model_ShouldMatchTheCapstoneContract()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new AppDbContext(options);

        var project = context.Model.FindEntityType(typeof(CapstoneProject));
        Assert.NotNull(project);
        Assert.Equal("CapstoneProjects", project.GetTableName());
        Assert.Equal(150, project.FindProperty(nameof(CapstoneProject.Title))!.GetMaxLength());
        Assert.Equal(4000, project.FindProperty(nameof(CapstoneProject.Description))!.GetMaxLength());
        AssertRestrictForeignKeys(project);
        AssertUnique(project, nameof(CapstoneProject.TrackId));
        AssertUnique(project, nameof(CapstoneProject.TaskId));

        var milestone = context.Model.FindEntityType(typeof(ProjectMilestone));
        Assert.NotNull(milestone);
        Assert.Equal("ProjectMilestones", milestone.GetTableName());
        Assert.Equal(150, milestone.FindProperty(nameof(ProjectMilestone.Title))!.GetMaxLength());
        Assert.Equal(2000, milestone.FindProperty(nameof(ProjectMilestone.Instructions))!.GetMaxLength());
        Assert.Equal(100, milestone.FindProperty(nameof(ProjectMilestone.WorkflowJobName))!.GetMaxLength());
        Assert.False(milestone.FindProperty(nameof(ProjectMilestone.WorkflowJobName))!.IsNullable);
        AssertRestrictForeignKeys(milestone);
        AssertUnique(milestone, nameof(ProjectMilestone.ProjectId), nameof(ProjectMilestone.Order));
        AssertUnique(milestone, nameof(ProjectMilestone.ProjectId), nameof(ProjectMilestone.WorkflowJobName));
    }

    [Fact]
    public void MigratedSchema_ShouldRenameTablesAndKeepTheConstraints()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);
        context.Database.Migrate();

        var projects = ReadSchema(connection, "table", "CapstoneProjects");
        var milestones = ReadSchema(connection, "table", "ProjectMilestones");

        Assert.NotNull(projects);
        Assert.NotNull(milestones);
        Assert.Null(ReadSchema(connection, "table", "CapstoneProject"));
        Assert.Null(ReadSchema(connection, "table", "ProjectMilestone"));

        Assert.Contains("CK_CapstoneProjects_Title_NotEmpty", projects);
        Assert.Contains("ON DELETE RESTRICT", projects);
        Assert.DoesNotContain("TrackId1", projects);
        Assert.DoesNotContain("TaskId1", projects);

        Assert.Contains("CK_ProjectMilestones_Title_NotEmpty", milestones);
        Assert.Contains("CK_ProjectMilestones_Order_Positive", milestones);
        Assert.Contains("CK_ProjectMilestones_WorkflowJobName_NotEmpty", milestones);
        Assert.Contains("ON DELETE RESTRICT", milestones);

        Assert.NotNull(ReadSchema(connection, "index", "IX_CapstoneProjects_TrackId"));
        Assert.NotNull(ReadSchema(connection, "index", "IX_CapstoneProjects_TaskId"));
        Assert.NotNull(ReadSchema(connection, "index", "IX_ProjectMilestones_ProjectId_Order"));
        Assert.NotNull(ReadSchema(connection, "index", "IX_ProjectMilestones_ProjectId_WorkflowJobName"));
    }

    private static void AssertRestrictForeignKeys(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        var foreignKeys = entityType.GetForeignKeys().ToList();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, foreignKey =>
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
        Assert.DoesNotContain(foreignKeys, foreignKey =>
            foreignKey.Properties.Any(property => property.Name.EndsWith("Id1", StringComparison.Ordinal)));
    }

    private static void AssertUnique(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, params string[] columns)
    {
        Assert.Contains(entityType.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(columns));
    }

    private static string? ReadSchema(SqliteConnection connection, string type, string name)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sql FROM sqlite_master WHERE type = $type AND name = $name";
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$name", name);

        return command.ExecuteScalar() as string;
    }
}
