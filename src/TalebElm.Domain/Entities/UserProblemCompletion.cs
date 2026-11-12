
namespace TalebElm.Domain.Entities;

public class UserProblemCompletion : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid ModuleProblemId { get; private set; }

    public string? ExternalSubmissionId { get; private set; }

    public DateTime VerifiedAtUtc { get; private set; }

    public string? DetailsJson { get; private set; }

    // GitHub verification fields
    public string? VerifiedHeadCommitSha { get; private set; }

    public Guid? TaskVersionId { get; private set; }

    public ModuleProblem ModuleProblem { get; private set; } = null!;

    private UserProblemCompletion()
    {
    }

    public UserProblemCompletion(
        Guid userId,
        Guid moduleProblemId,
        DateTime verifiedAtUtc,
        string? externalSubmissionId = null,
        string? detailsJson = null,
        string? verifiedHeadCommitSha = null,
        Guid? taskVersionId = null)
    {
        UserId = userId;
        ModuleProblemId = moduleProblemId;
        VerifiedAtUtc = verifiedAtUtc;
        ExternalSubmissionId = externalSubmissionId;
        DetailsJson = detailsJson;
        VerifiedHeadCommitSha = verifiedHeadCommitSha;
        TaskVersionId = taskVersionId;
    }
}
