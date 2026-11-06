namespace TalebElm.Application.DTOs.External;

/// <summary>
/// A GitHub user account.
/// </summary>
/// <param name="Id">The numeric GitHub account id.</param>
/// <param name="Login">The account login (username).</param>
public record GitHubUser(
    long Id,
    string Login);

/// <summary>
/// A GitHub repository.
/// </summary>
/// <param name="Id">The numeric repository id.</param>
/// <param name="Name">The repository name without the owner.</param>
/// <param name="FullName">The repository name in <c>owner/repo</c> form.</param>
/// <param name="Owner">The repository owner.</param>
/// <param name="Fork">Whether the repository is a fork.</param>
/// <param name="DefaultBranch">The default branch name, or <c>null</c> when GitHub does not report one.</param>
public record GitHubRepository(
    long Id,
    string Name,
    string FullName,
    GitHubUser Owner,
    bool Fork,
    string? DefaultBranch);

/// <summary>
/// A GitHub Actions workflow run.
/// </summary>
/// <param name="Id">The numeric workflow run id.</param>
/// <param name="Name">
/// The run name, or <c>null</c> when the workflow has no name. The name is display data only and
/// must never be used to identify a workflow.
/// </param>
/// <param name="HeadSha">The commit SHA the run was executed against.</param>
/// <param name="Status">
/// The run status, for example <c>queued</c>, <c>in_progress</c> or <c>completed</c>. A run is only
/// conclusive once this is <c>completed</c>.
/// </param>
/// <param name="Conclusion">
/// The run conclusion once completed, for example <c>success</c>, <c>failure</c> or <c>cancelled</c>;
/// otherwise <c>null</c>.
/// </param>
/// <param name="WorkflowId">The numeric workflow id. GitHub returns this as a number, not a file name.</param>
public record GitHubWorkflowRun(
    long Id,
    string? Name,
    string HeadSha,
    string Status,
    string? Conclusion,
    long WorkflowId);

/// <summary>
/// A single entry in a Git tree.
/// </summary>
/// <param name="Path">The path relative to the repository root.</param>
/// <param name="Mode">The Git file mode, for example <c>100644</c>.</param>
/// <param name="Type">The Git object type, for example <c>blob</c> or <c>tree</c>.</param>
/// <param name="Sha">The object SHA, used for integrity checks.</param>
/// <param name="Size">The blob size in bytes, or <c>null</c> for trees and submodules.</param>
/// <param name="Url">The API URL of the object, if provided.</param>
public record GitHubTreeItem(
    string Path,
    string Mode,
    string Type,
    string Sha,
    long? Size = null,
    string? Url = null);

/// <summary>
/// A Git tree response.
/// </summary>
/// <param name="Sha">The SHA of the tree itself.</param>
/// <param name="Url">The API URL of the tree.</param>
/// <param name="Tree">The entries in the tree.</param>
/// <param name="Truncated">
/// Whether GitHub truncated the response because the tree is too large. When <c>true</c> the entries are
/// incomplete, so a missing path is not proof that the path is absent; verification must fail closed
/// (or fetch the missing subtrees) rather than report the repository as invalid.
/// </param>
public record GitHubGitTree(
    string Sha,
    string Url,
    IReadOnlyList<GitHubTreeItem> Tree,
    bool Truncated);
