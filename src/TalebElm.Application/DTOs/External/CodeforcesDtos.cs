namespace TalebElm.Application.DTOs.External;

/// <summary>
/// A Codeforces user profile returned by <c>user.info</c>.
/// </summary>
/// <param name="Handle">The Codeforces handle exactly as returned by the API.</param>
/// <param name="FirstName">The profile first name, if the user set one.</param>
/// <param name="Organization">The profile organisation, if the user set one.</param>
/// <param name="Rating">The current rating, or <c>null</c> for unrated users.</param>
/// <param name="Rank">The current rank, or <c>null</c> for unrated users.</param>
public record CodeforcesUser(
    string Handle,
    string? FirstName,
    string? Organization,
    int? Rating,
    string? Rank);

/// <summary>
/// The problem reference embedded in a Codeforces submission.
/// </summary>
/// <param name="ContestId">The contest id, or <c>null</c> for problems outside a contest (for example gym/archive problems).</param>
/// <param name="Index">The problem index within the contest, for example <c>A</c> or <c>B1</c>.</param>
/// <param name="Name">The problem name.</param>
/// <param name="Rating">The problem rating, or <c>null</c> when unrated.</param>
/// <param name="Tags">The problem tags, or <c>null</c> when the API omits them.</param>
public record CodeforcesProblemInfo(
    int? ContestId,
    string Index,
    string Name,
    int? Rating,
    IReadOnlyList<string>? Tags = null);

/// <summary>
/// A single submission returned by <c>user.status</c>.
/// </summary>
/// <param name="Id">The Codeforces submission id.</param>
/// <param name="ContestId">The contest id, or <c>null</c> for problems outside a contest.</param>
/// <param name="Problem">The problem the submission targets.</param>
/// <param name="Verdict">
/// The judging verdict, for example <c>OK</c>, or <c>null</c> while the submission is still in the
/// judging queue. A <c>null</c> verdict is never a success and must not be treated as <c>OK</c>.
/// </param>
/// <param name="PassedTestCount">The number of tests the submission passed.</param>
/// <param name="CreationTimeSeconds">The submission time as a Unix timestamp in seconds.</param>
public record CodeforcesSubmission(
    long Id,
    int? ContestId,
    CodeforcesProblemInfo Problem,
    string? Verdict,
    int PassedTestCount,
    long CreationTimeSeconds);
