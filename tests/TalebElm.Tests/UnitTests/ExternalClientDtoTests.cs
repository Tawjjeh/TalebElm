using Moq;
using TalebElm.Application.DTOs.External;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Tests.UnitTests;

public class ExternalClientDtoTests
{
    [Fact]
    public void CodeforcesDtos_InstantiateAndExposeProperties()
    {
        var user = new CodeforcesUser("tourist", "Gennady", "ITMO", 3800, "legendary grandmaster");
        Assert.Equal("tourist", user.Handle);
        Assert.Equal("Gennady", user.FirstName);
        Assert.Equal("ITMO", user.Organization);
        Assert.Equal(3800, user.Rating);
        Assert.Equal("legendary grandmaster", user.Rank);

        var problem = new CodeforcesProblemInfo(158, "B", "Taxi", 1100, ["greedy", "math"]);
        Assert.Equal(158, problem.ContestId);
        Assert.Equal("B", problem.Index);
        Assert.Equal("Taxi", problem.Name);
        Assert.Equal(1100, problem.Rating);
        Assert.NotNull(problem.Tags);
        Assert.Equal(2, problem.Tags.Count);

        var submission = new CodeforcesSubmission(123456, 158, problem, "OK", 45, 1700000000);
        Assert.Equal(123456, submission.Id);
        Assert.Equal(158, submission.ContestId);
        Assert.Equal("OK", submission.Verdict);
        Assert.Equal(45, submission.PassedTestCount);
        Assert.Equal(1700000000, submission.CreationTimeSeconds);
        Assert.Equal("Taxi", submission.Problem.Name);
    }

    [Fact]
    public void GitHubDtos_InstantiateAndExposeProperties()
    {
        var owner = new GitHubUser(1001, "octocat");
        Assert.Equal(1001, owner.Id);
        Assert.Equal("octocat", owner.Login);

        var repo = new GitHubRepository(2001, "my-repo", "octocat/my-repo", owner, false, "main");
        Assert.Equal(2001, repo.Id);
        Assert.Equal("my-repo", repo.Name);
        Assert.Equal("octocat/my-repo", repo.FullName);
        Assert.Equal(owner, repo.Owner);
        Assert.False(repo.Fork);
        Assert.Equal("main", repo.DefaultBranch);

        var run = new GitHubWorkflowRun(3001, "Test Workflow", "a1b2c3d4e5", "completed", "success", "test.yml");
        Assert.Equal(3001, run.Id);
        Assert.Equal("Test Workflow", run.Name);
        Assert.Equal("a1b2c3d4e5", run.HeadSha);
        Assert.Equal("completed", run.Status);
        Assert.Equal("success", run.Conclusion);
        Assert.Equal("test.yml", run.WorkflowId);

        var treeItem = new GitHubTreeItem("tests/UnitTest.cs", "100644", "blob", "blobsha123", 1024, "https://api.github.com/blob");
        Assert.Equal("tests/UnitTest.cs", treeItem.Path);
        Assert.Equal("100644", treeItem.Mode);
        Assert.Equal("blob", treeItem.Type);
        Assert.Equal("blobsha123", treeItem.Sha);
        Assert.Equal(1024, treeItem.Size);

        var gitTree = new GitHubGitTree("treesha456", "https://api.github.com/trees", [treeItem], false);
        Assert.Equal("treesha456", gitTree.Sha);
        Assert.Single(gitTree.Tree);
        Assert.False(gitTree.Truncated);
    }

    [Fact]
    public async Task Clients_CanBeMockedWithCancellationToken()
    {
        var mockCf = new Mock<ICodeforcesClient>();
        mockCf.Setup(c => c.GetUserProfileAsync("tourist", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CodeforcesUser("tourist", "Gennady", null, null, null));

        var user = await mockCf.Object.GetUserProfileAsync("tourist", CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("tourist", user.Handle);

        var mockGh = new Mock<IGitHubClient>();
        mockGh.Setup(g => g.GetRepositoryAsync("octocat", "hello", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GitHubRepository(1, "hello", "octocat/hello", new GitHubUser(1, "octocat"), false, "main"));

        var repo = await mockGh.Object.GetRepositoryAsync("octocat", "hello", CancellationToken.None);
        Assert.NotNull(repo);
        Assert.Equal("octocat/hello", repo.FullName);
    }
}
