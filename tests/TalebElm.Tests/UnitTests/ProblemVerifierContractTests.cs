using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ProblemVerifierContractTests
{
    [Fact]
    public void ProblemVerificationResult_HasExpectedShape()
    {
        var versionId = Guid.NewGuid();

        var result = new VerificationOutcome(
            true, VerificationFailureReason.None, versionId, "abc123");

        Assert.True(result.Success);
        Assert.Equal(VerificationFailureReason.None, result.Reason);
        Assert.Equal(versionId, result.TaskVersionId);
        Assert.Equal("abc123", result.VerifiedHeadCommitSha);
    }

    [Fact]
    public async Task VerifyAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var verifier = new FakeProblemVerifier();

        await verifier.VerifyAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);

        Assert.Equal(cts.Token, verifier.ReceivedToken);
    }

    private class FakeProblemVerifier : IProblemVerifier
    {
        public CancellationToken ReceivedToken;

        public Task<VerificationOutcome> VerifyAsync(
            Guid userId, Guid moduleProblemId, CancellationToken ct = default)
        {
            ReceivedToken = ct;
            return Task.FromResult(new VerificationOutcome(
                true, VerificationFailureReason.None, null, null));
        }
    }
}
