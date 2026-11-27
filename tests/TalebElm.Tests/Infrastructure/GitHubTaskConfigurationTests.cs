using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TalebElm.Domain.Entities;

namespace TalebElm.Tests.Infrastructure;

public class GitHubTaskConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigurePropertiesAndPreserveTheSingularTableName()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(GitHubTask));

        Assert.NotNull(entityType);
        Assert.Equal("GitHubTask", entityType.GetTableName());

        var templateRepository = entityType.FindProperty(nameof(GitHubTask.TemplateRepository));
        Assert.NotNull(templateRepository);
        Assert.False(templateRepository.IsNullable);
        Assert.Equal(200, templateRepository.GetMaxLength());

        var workflowFileName = entityType.FindProperty(nameof(GitHubTask.WorkflowFileName));
        Assert.NotNull(workflowFileName);
        Assert.False(workflowFileName.IsNullable);
        Assert.Equal(200, workflowFileName.GetMaxLength());

        var minPassingTests = entityType.FindProperty(nameof(GitHubTask.MinPassingTests));
        Assert.NotNull(minPassingTests);
        Assert.True(minPassingTests.IsNullable);

        var isActive = entityType.FindProperty(nameof(GitHubTask.IsActive));
        Assert.NotNull(isActive);
        Assert.False(isActive.IsNullable);
    }

    [Fact]
    public void Model_ShouldConfigureGitHubTaskCheckConstraints()
    {
        var designTimeModel = DbContext.GetService<IDesignTimeModel>().Model;
        var entityType = designTimeModel.FindEntityType(typeof(GitHubTask));

        Assert.NotNull(entityType);
        var constraints = entityType.GetCheckConstraints().ToList();

        Assert.Contains(constraints, constraint =>
            constraint.Name == "CK_GitHubTask_TemplateRepository_NotEmpty"
            && constraint.Sql == "length(trim(TemplateRepository)) > 0");

        Assert.Contains(constraints, constraint =>
            constraint.Name == "CK_GitHubTask_MinPassingTests_Positive"
            && constraint.Sql == "MinPassingTests IS NULL OR MinPassingTests > 0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(42)]
    public async Task SaveChangesAsync_WithNullOrPositiveMinPassingTests_IsAllowed(int? minPassingTests)
    {
        DbContext.GitHubTasks.Add(new GitHubTask
        {
            Id = Guid.NewGuid(),
            TemplateRepository = "Tawjjeh/task-key-value-store-template",
            MinPassingTests = minPassingTests
        });

        await SaveChangesAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SaveChangesAsync_WithNonPositiveMinPassingTests_ThrowsDbUpdateException(int minPassingTests)
    {
        DbContext.GitHubTasks.Add(new GitHubTask
        {
            Id = Guid.NewGuid(),
            TemplateRepository = "Tawjjeh/task-key-value-store-template",
            MinPassingTests = minPassingTests
        });

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task SaveChangesAsync_WithBlankTemplateRepository_ThrowsDbUpdateException()
    {
        DbContext.GitHubTasks.Add(new GitHubTask
        {
            Id = Guid.NewGuid(),
            TemplateRepository = "   "
        });

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }
}
