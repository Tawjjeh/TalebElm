using TalebElm.Application.DTOs.External;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Application.Services.Abstraction;

/// <summary>
/// Read-only access to the GitHub REST API. Implementations live in Infrastructure and use a typed
/// <see cref="HttpClient"/>; business rules stay in the Application layer.
/// <para>
/// All parameters that come from a learner (<c>owner</c>, <c>repo</c>, <c>workflow</c>,
/// <c>treeSha</c>, <c>branch</c>) are untrusted. Implementations must validate them (non-empty,
/// length limits, allowed characters) and URL-escape every path segment before building a request.
/// They must never concatenate raw input into a URL.
/// </para>
/// <para>
/// The methods distinguish "not found" from "temporarily unavailable". A not-found result is
/// <c>null</c>; an outage, rate limit, timeout or an unparseable response is an
/// <see cref="ExternalServiceUnavailableException"/>. A failure must never look like "repository not
/// found", otherwise a learner could fail task verification during a GitHub outage.
/// </para>
/// </summary>
public interface IGitHubClient
{
    /// <summary>
    /// Gets a repository by owner and name.
    /// </summary>
    /// <param name="owner">The repository owner. Untrusted; implementations validate and escape it.</param>
    /// <param name="repo">The repository name. Untrusted; implementations validate and escape it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The repository, or <c>null</c> when GitHub positively reports that it does not exist (or is not
    /// visible to the caller).
    /// </returns>
    /// <exception cref="ExternalServiceUnavailableException">
    /// GitHub is unreachable, rate limits the request, times out, or returns a malformed response.
    /// </exception>
    Task<GitHubRepository?> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest workflow run for a specific workflow on a specific branch.
    /// </summary>
    /// <param name="owner">The repository owner. Untrusted; implementations validate and escape it.</param>
    /// <param name="repo">The repository name. Untrusted; implementations validate and escape it.</param>
    /// <param name="workflow">
    /// The workflow file name or numeric workflow id accepted by the GitHub API (for example
    /// <c>ci.yml</c>). This is not <see cref="GitHubWorkflowRun.WorkflowId"/>. Untrusted; implementations
    /// validate and escape it.
    /// </param>
    /// <param name="branch">The branch to filter by. Required, so a run from another branch can never be
    /// mistaken for the learner's default-branch run. Untrusted; implementations validate and escape it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The most recent run for that workflow and branch, regardless of status, or <c>null</c> when no run
    /// exists. Callers must inspect <see cref="GitHubWorkflowRun.Status"/> and
    /// <see cref="GitHubWorkflowRun.Conclusion"/>: only <c>completed</c> with <c>success</c> counts as a
    /// pass, and an <c>in_progress</c> or <c>queued</c> run is not a failure either.
    /// </returns>
    /// <exception cref="ExternalServiceUnavailableException">
    /// GitHub is unreachable, rate limits the request, times out, or returns a malformed response.
    /// </exception>
    Task<GitHubWorkflowRun?> GetLatestWorkflowRunAsync(string owner, string repo, string workflow, string branch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Git tree, recursively by default, so the complete repository layout is available for
    /// blob-SHA verification.
    /// </summary>
    /// <param name="owner">The repository owner. Untrusted; implementations validate and escape it.</param>
    /// <param name="repo">The repository name. Untrusted; implementations validate and escape it.</param>
    /// <param name="treeSha">The tree or commit SHA to read. Untrusted; implementations validate and escape it.</param>
    /// <param name="recursive">
    /// When <c>true</c> (the default) the request uses <c>recursive=1</c> so nested files are returned in
    /// one response. When <c>false</c> only the top level is returned.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The tree, or <c>null</c> when GitHub positively reports that it does not exist.
    /// </returns>
    /// <remarks>
    /// GitHub can truncate a large recursive response. When <see cref="GitHubGitTree.Truncated"/> is
    /// <c>true</c> the entries are incomplete, so a missing path is not proof that the path is absent.
    /// Callers must fail closed (or fetch the missing subtrees) instead of reporting the repository as
    /// invalid.
    /// </remarks>
    /// <exception cref="ExternalServiceUnavailableException">
    /// GitHub is unreachable, rate limits the request, times out, or returns a malformed response.
    /// </exception>
    Task<GitHubGitTree?> GetGitTreeAsync(string owner, string repo, string treeSha, bool recursive = true, CancellationToken cancellationToken = default);
}
