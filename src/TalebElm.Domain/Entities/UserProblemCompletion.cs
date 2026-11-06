namespace TalebElm.Domain.Entities;

public class UserProblemCompletion : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid ModuleProblemId { get; private set; }

    public string ExternalSubmissionId { get; private set; } = null!;

    public DateTime VerifiedAtUtc { get; private set; }

    public string? DetailsJson { get; private set; }

    public ModuleProblem ModuleProblem { get; private set; } = null!;

    // Required by EF Core
    private UserProblemCompletion()
    {
    }

    public UserProblemCompletion(
        Guid userId,
        Guid moduleProblemId,
        string externalSubmissionId,
        DateTime verifiedAtUtc,
        string? detailsJson = null)
    {
        UserId = userId;
        ModuleProblemId = moduleProblemId;
        ExternalSubmissionId = externalSubmissionId;
        VerifiedAtUtc = verifiedAtUtc;
        DetailsJson = detailsJson;
    }
}