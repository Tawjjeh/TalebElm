using Microsoft.Extensions.Options;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Api.BackgroundServices;

public sealed class LinkHealthCheckHostedService(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<LinkHealthCheckOptions> options,
    ILogger<LinkHealthCheckHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (environment.IsEnvironment("Testing") || !options.Value.Enabled)
        {
            logger.LogInformation(
                "Scheduled link-health checker skipped (Environment={Environment}, Enabled={Enabled}).",
                environment.EnvironmentName,
                options.Value.Enabled);
            return;
        }

        var interval = options.Value.Interval > TimeSpan.Zero
            ? options.Value.Interval
            : TimeSpan.FromDays(1);

        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSingleSweepAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    internal async Task RunSingleSweepAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var sweepService = scope.ServiceProvider.GetRequiredService<IResourceLinkHealthSweepService>();
            var summary = await sweepService.RunSweepAsync(ct);

            logger.LogInformation(
                "Link-health sweep completed: Checked={Checked}, Healthy={Healthy}, Failed={Failed}, MarkedBroken={MarkedBroken}, Recovered={Recovered}.",
                summary.CheckedCount,
                summary.HealthyCount,
                summary.FailedCount,
                summary.MarkedBrokenCount,
                summary.RecoveredCount);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure during scheduled link-health sweep.");
        }
    }
}
