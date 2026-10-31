namespace TalebElm.Application.DTOs.External;

public record CodeforcesUser(
    string Handle,
    string? FirstName,
    string? Organization,
    int? Rating,
    string? Rank);

public record CodeforcesProblemInfo(
    int? ContestId,
    string Index,
    string Name,
    int? Rating,
    IReadOnlyList<string>? Tags = null);

public record CodeforcesSubmission(
    long Id,
    int? ContestId,
    CodeforcesProblemInfo Problem,
    string Verdict,
    int PassedTestCount,
    long CreationTimeSeconds);
