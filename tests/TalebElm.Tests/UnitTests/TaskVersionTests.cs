using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TaskVersionTests
{
    [Fact]
    public void TaskVersion_HasExpectedDefaultValues()
    {
        var version = new TaskVersion();

        Assert.NotEqual(Guid.Empty, version.Id);
        Assert.True(version.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.Equal(Guid.Empty, version.GitHubTaskId);
        Assert.Null(version.GitHubTask);
        Assert.Equal(string.Empty, version.VersionTag);
        Assert.Equal(string.Empty, version.CommitSha);
        Assert.Equal(string.Empty, version.WorkflowFileSha);
        Assert.Equal(string.Empty, version.TestTreeSha);
        Assert.Equal(TaskVersionStatus.Active, version.Status);
    }

    [Fact]
    public void TaskVersion_PropertiesCanBeAssigned()
    {
        var task = new GitHubTask();

        var version = new TaskVersion
        {
            GitHubTaskId = task.Id,
            GitHubTask = task,
            VersionTag = "v1.0.0",
            CommitSha = "0123456789abcdef0123456789abcdef01234567",
            WorkflowFileSha = "89abcdef0123456789abcdef0123456789abcdef",
            TestTreeSha = "fedcba9876543210fedcba9876543210fedcba98",
            Status = TaskVersionStatus.Deprecated
        };

        Assert.Equal(task.Id, version.GitHubTaskId);
        Assert.Same(task, version.GitHubTask);
        Assert.Equal("v1.0.0", version.VersionTag);
        Assert.Equal("0123456789abcdef0123456789abcdef01234567", version.CommitSha);
        Assert.Equal("89abcdef0123456789abcdef0123456789abcdef", version.WorkflowFileSha);
        Assert.Equal("fedcba9876543210fedcba9876543210fedcba98", version.TestTreeSha);
        Assert.Equal(TaskVersionStatus.Deprecated, version.Status);
    }
}
