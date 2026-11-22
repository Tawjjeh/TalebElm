using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TaskVerificationServiceContractTests
{
    [Fact]
    public void TaskVerificationResult_HasExpectedShape()
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
    public async Task VerifyRepositoryAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var service = new FakeTaskVerificationService();

        await service.VerifyRepositoryAsync(
            Guid.NewGuid(), Guid.NewGuid(), "repo", null, cts.Token);

        Assert.Equal(cts.Token, service.ReceivedToken);
    }

    private class FakeTaskVerificationService : ITaskVerificationService
    {
        public CancellationToken ReceivedToken;

        public Task<VerificationOutcome> VerifyRepositoryAsync(
            Guid userId, Guid gitHubTaskId, string repositoryName,
            string? requiredJobName = null, CancellationToken ct = default)
        {
            ReceivedToken = ct;
            return Task.FromResult(new VerificationOutcome(
                true, VerificationFailureReason.None, null, null));
        }
    }
}
