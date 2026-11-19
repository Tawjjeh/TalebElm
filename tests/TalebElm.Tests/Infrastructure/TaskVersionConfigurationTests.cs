using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class TaskVersionConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigurePropertiesForeignKeysAndIndexes()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(TaskVersion));

        Assert.NotNull(entityType);
        Assert.Equal("TaskVersions", entityType.GetTableName());

        var versionTag = entityType.FindProperty(nameof(TaskVersion.VersionTag));
        Assert.NotNull(versionTag);
        Assert.False(versionTag.IsNullable);
        Assert.Equal(50, versionTag.GetMaxLength());

        foreach (var propertyName in new[]
                 {
                     nameof(TaskVersion.CommitSha),
                     nameof(TaskVersion.WorkflowFileSha),
                     nameof(TaskVersion.TestTreeSha)
                 })
        {
            var property = entityType.FindProperty(propertyName);
            Assert.NotNull(property);
            Assert.False(property.IsNullable);
            Assert.Equal(40, property.GetMaxLength());
        }

        var status = entityType.FindProperty(nameof(TaskVersion.Status));
        Assert.NotNull(status);
        Assert.False(status.IsNullable);

        var foreignKey = Assert.Single(entityType.GetForeignKeys());
        Assert.Equal(typeof(GitHubTask), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(TaskVersion.GitHubTaskId), Assert.Single(foreignKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);

        var activeVersionIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(TaskVersion.GitHubTaskId) }));

        Assert.True(activeVersionIndex.IsUnique);
        Assert.Equal("Status = 1", activeVersionIndex.GetFilter());

        var versionTagIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(new[]
                {
                    nameof(TaskVersion.GitHubTaskId),
                    nameof(TaskVersion.VersionTag)
                }));

        Assert.True(versionTagIndex.IsUnique);
        Assert.Null(versionTagIndex.GetFilter());
    }

    [Fact]
    public void Migration_ShouldPreserveGitHubTaskAndCreateTaskVersionsWithTheFilteredIndex()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);
        context.Database.Migrate();

        var gitHubTask = ReadSchema(connection, "table", "GitHubTask");
        Assert.NotNull(gitHubTask);
        Assert.Null(ReadSchema(connection, "table", "GitHubTasks"));
        Assert.Contains("CK_GitHubTask_TemplateRepository_NotEmpty", gitHubTask);
        Assert.Contains("CK_GitHubTask_MinPassingTests_Positive", gitHubTask);

        var taskVersions = ReadSchema(connection, "table", "TaskVersions");
        Assert.NotNull(taskVersions);
        Assert.Contains("ON DELETE RESTRICT", taskVersions);

        var activeVersionIndex = ReadSchema(connection, "index", "IX_TaskVersions_GitHubTaskId");
        Assert.NotNull(activeVersionIndex);
        Assert.Contains("WHERE Status = 1", activeVersionIndex);

        Assert.NotNull(ReadSchema(connection, "index", "IX_TaskVersions_GitHubTaskId_VersionTag"));
    }

    [Fact]
    public async Task TwoActiveVersionsForTheSameTask_ThrowDbUpdateException()
    {
        var task = await CreateTaskAsync();

        DbContext.TaskVersions.AddRange(
            NewVersion(task.Id, "v1.0.0", TaskVersionStatus.Active),
            NewVersion(task.Id, "v1.0.1", TaskVersionStatus.Active));

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task OneActiveVersionWithDeprecatedAndRetiredVersions_IsAllowed()
    {
        var task = await CreateTaskAsync();

        DbContext.TaskVersions.AddRange(
            NewVersion(task.Id, "v1.0.0", TaskVersionStatus.Active),
            NewVersion(task.Id, "v1.0.1", TaskVersionStatus.Deprecated),
            NewVersion(task.Id, "v1.0.2", TaskVersionStatus.Retired));

        await SaveChangesAsync();
        ClearTracker();

        Assert.Equal(3, await DbContext.TaskVersions.CountAsync(version => version.GitHubTaskId == task.Id));
    }

    [Fact]
    public async Task DuplicateVersionTagForTheSameTask_ThrowsDbUpdateException()
    {
        var task = await CreateTaskAsync();

        DbContext.TaskVersions.AddRange(
            NewVersion(task.Id, "v1.0.0", TaskVersionStatus.Deprecated),
            NewVersion(task.Id, "v1.0.0", TaskVersionStatus.Retired));

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task DifferentTasksCanEachHaveAnActiveVersion()
    {
        var firstTask = await CreateTaskAsync();
        var secondTask = await CreateTaskAsync();

        DbContext.TaskVersions.AddRange(
            NewVersion(firstTask.Id, "v1.0.0", TaskVersionStatus.Active),
            NewVersion(secondTask.Id, "v1.0.0", TaskVersionStatus.Active));

        await SaveChangesAsync();
    }

    [Fact]
    public async Task DeletingATaskWithVersions_ThrowsDbUpdateException()
    {
        var task = await CreateTaskAsync();
        DbContext.TaskVersions.Add(NewVersion(task.Id, "v1.0.0", TaskVersionStatus.Active));
        await SaveChangesAsync();
        ClearTracker();

        var taskToDelete = await DbContext.GitHubTasks.FindAsync(task.Id);
        Assert.NotNull(taskToDelete);
        DbContext.GitHubTasks.Remove(taskToDelete);

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    private async Task<GitHubTask> CreateTaskAsync()
    {
        var task = new GitHubTask
        {
            Id = Guid.NewGuid(),
            TemplateRepository = "Tawjjeh/task-key-value-store-template"
        };

        DbContext.GitHubTasks.Add(task);
        await SaveChangesAsync();
        ClearTracker();
        return task;
    }

    private static TaskVersion NewVersion(Guid taskId, string versionTag, TaskVersionStatus status)
        => new()
        {
            Id = Guid.NewGuid(),
            GitHubTaskId = taskId,
            VersionTag = versionTag,
            CommitSha = new string('a', 40),
            WorkflowFileSha = new string('b', 40),
            TestTreeSha = new string('c', 40),
            Status = status
        };

    private static string? ReadSchema(SqliteConnection connection, string type, string name)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sql FROM sqlite_master WHERE type = $type AND name = $name";
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$name", name);

        return command.ExecuteScalar() as string;
    }
}
