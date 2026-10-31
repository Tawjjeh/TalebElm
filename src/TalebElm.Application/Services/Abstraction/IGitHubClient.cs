using TalebElm.Application.DTOs.External;

namespace TalebElm.Application.Services.Abstraction;

public interface IGitHubClient
{
    Task<GitHubRepository?> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default);

    Task<GitHubWorkflowRun?> GetLatestWorkflowRunAsync(string owner, string repo, string workflow, CancellationToken cancellationToken = default);

    Task<GitHubGitTree?> GetGitTreeAsync(string owner, string repo, string treeSha, CancellationToken cancellationToken = default);
}
