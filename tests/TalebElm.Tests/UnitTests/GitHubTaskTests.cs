using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class GitHubTaskTests
{
    [Fact]
    public void GitHubTask_HasExpectedDefaultValues()
    {
        var task = new GitHubTask();

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.True(task.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.Equal(string.Empty, task.TemplateRepository);
        Assert.Equal("test.yml", task.WorkflowFileName);
        Assert.Null(task.MinPassingTests);
        Assert.True(task.IsActive);
    }
}
