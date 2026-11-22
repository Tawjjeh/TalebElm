using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TalebElm.Api.BackgroundServices;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class ResourceLinkHealthSweepServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IResourceRepository> _resources = new();
    private readonly Mock<ILinkHealthChecker> _checker = new();
    private readonly ResourceLinkHealthSweepService _sut;

    public ResourceLinkHealthSweepServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Resources).Returns(_resources.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new ResourceLinkHealthSweepService(
            _unitOfWork.Object,
            _checker.Object,
            NullLogger<ResourceLinkHealthSweepService>.Instance);
    }

    [Fact]
    public async Task RunSweepAsync_ThreeConsecutiveFailures_MarksResourceBrokenOnThirdFailure()
    {
        var resource = new Resource
        {
            Title = "Broken Doc",
            Url = "https://example.com/404",
            Status = ResourceStatus.Approved,
            ConsecutiveFailureCount = 0
        };

        _resources.Setup(r => r.GetForHealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([resource]);
        _checker.Setup(c => c.CheckAsync(resource.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LinkCheckResult(false, 404, "HttpStatus_404"));

        // Failure 1 -> stays Approved
        var s1 = await _sut.RunSweepAsync();
        Assert.Equal(1, resource.ConsecutiveFailureCount);
        Assert.Equal(ResourceStatus.Approved, resource.Status);
        Assert.Equal(0, s1.MarkedBrokenCount);
        Assert.NotNull(resource.LastCheckedAt);

        // Failure 2 -> stays Approved
        var s2 = await _sut.RunSweepAsync();
        Assert.Equal(2, resource.ConsecutiveFailureCount);
        Assert.Equal(ResourceStatus.Approved, resource.Status);
        Assert.Equal(0, s2.MarkedBrokenCount);

        // Failure 3 -> flips to Broken
        var s3 = await _sut.RunSweepAsync();
        Assert.Equal(3, resource.ConsecutiveFailureCount);
        Assert.Equal(ResourceStatus.Broken, resource.Status);
        Assert.Equal(1, s3.MarkedBrokenCount);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RunSweepAsync_SuccessAfterFailures_ResetsCounterAndRecoversBrokenToApproved()
    {
        var resource = new Resource
        {
            Title = "Recovered Doc",
            Url = "https://example.com/recovered",
            Status = ResourceStatus.Broken,
            ConsecutiveFailureCount = 3
        };

        _resources.Setup(r => r.GetForHealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([resource]);
        _checker.Setup(c => c.CheckAsync(resource.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LinkCheckResult(true, 200));

        var summary = await _sut.RunSweepAsync();

        Assert.Equal(0, resource.ConsecutiveFailureCount);
        Assert.Equal(ResourceStatus.Approved, resource.Status);
        Assert.NotNull(resource.LastCheckedAt);
        Assert.Equal(1, summary.HealthyCount);
        Assert.Equal(1, summary.RecoveredCount);
        Assert.Equal(0, summary.FailedCount);
    }

    [Fact]
    public async Task RunSweepAsync_SkipsResourcesWithoutUrl()
    {
        var citationOnly = new Resource
        {
            Title = "Paper Book",
            Url = null,
            Citation = "Clean Code, Ch. 3",
            Status = ResourceStatus.Approved
        };

        _resources.Setup(r => r.GetForHealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([citationOnly]);

        var summary = await _sut.RunSweepAsync();

        Assert.Equal(0, summary.CheckedCount);
        _checker.Verify(c => c.CheckAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HostedService_InTestingEnvironment_SkipsSweepAutomatically()
    {
        var sweepMock = new Mock<IResourceLinkHealthSweepService>();
        var services = new ServiceCollection();
        services.AddScoped(_ => sweepMock.Object);
        using var provider = services.BuildServiceProvider();

        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Testing");

        var hosted = new LinkHealthCheckHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            env.Object,
            Options.Create(new LinkHealthCheckOptions { Enabled = true, Interval = TimeSpan.FromMilliseconds(20) }),
            NullLogger<LinkHealthCheckHostedService>.Instance);

        await hosted.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await hosted.StopAsync(CancellationToken.None);

        sweepMock.Verify(s => s.RunSweepAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HostedService_WhenDisabledInOptions_SkipsSweepAutomatically()
    {
        var sweepMock = new Mock<IResourceLinkHealthSweepService>();
        var services = new ServiceCollection();
        services.AddScoped(_ => sweepMock.Object);
        using var provider = services.BuildServiceProvider();

        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Production");

        var hosted = new LinkHealthCheckHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            env.Object,
            Options.Create(new LinkHealthCheckOptions { Enabled = false, Interval = TimeSpan.FromMilliseconds(20) }),
            NullLogger<LinkHealthCheckHostedService>.Instance);

        await hosted.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await hosted.StopAsync(CancellationToken.None);

        sweepMock.Verify(s => s.RunSweepAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HostedService_WhenEnabledInProduction_RunsSweepInsideDiScope()
    {
        var ran = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sweepMock = new Mock<IResourceLinkHealthSweepService>();
        sweepMock.Setup(s => s.RunSweepAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                ran.TrySetResult();
                return new LinkHealthSweepSummary(1, 1, 0, 0, 0);
            });

        var services = new ServiceCollection();
        services.AddScoped(_ => sweepMock.Object);
        using var provider = services.BuildServiceProvider();

        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Production");

        var hosted = new LinkHealthCheckHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            env.Object,
            Options.Create(new LinkHealthCheckOptions { Enabled = true, Interval = TimeSpan.FromHours(1) }),
            NullLogger<LinkHealthCheckHostedService>.Instance);

        await hosted.StartAsync(CancellationToken.None);
        await ran.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await hosted.StopAsync(CancellationToken.None);

        sweepMock.Verify(s => s.RunSweepAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
