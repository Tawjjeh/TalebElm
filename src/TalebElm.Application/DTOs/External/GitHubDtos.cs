namespace TalebElm.Application.DTOs.External;

public record GitHubUser(
    long Id,
    string Login);

public record GitHubRepository(
    long Id,
    string Name,
    string FullName,
    GitHubUser Owner,
    bool Fork,
    string? DefaultBranch);

public record GitHubWorkflowRun(
    long Id,
    string Name,
    string HeadSha,
    string Status,
    string? Conclusion,
    string? WorkflowId = null);

public record GitHubTreeItem(
    string Path,
    string Mode,
    string Type,
    string Sha,
    long? Size = null,
    string? Url = null);

public record GitHubGitTree(
    string Sha,
    string Url,
    IReadOnlyList<GitHubTreeItem> Tree,
    bool Truncated);
