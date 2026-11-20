using TalebElm.Domain.Enums;

namespace TalebElm.Application.DTOs;

public sealed record ProblemVerificationResult(
    bool Success,
    VerificationFailureReason Reason,
    Guid? TaskVersionId,
    string? VerifiedHeadCommitSha
);
