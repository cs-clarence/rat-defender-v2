using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.HostedServices;

public class RatDetectionBackgroundService(
    ILogger<RatDetectionBackgroundService> logger,
    IServiceProvider provider,
    IBuzzer buzzer,
    IDetectionNotifier notifier
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(50, stoppingToken);
                await RunAsync(stoppingToken);
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException) return;
            
            logger.LogError(e,
                "An error occurred while running RatDetectionBackgroundService");
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        ulong detectionCount;
        using (var scope = provider.CreateScope())
        {
            var detector = scope.ServiceProvider
                .GetRequiredService<IRatDetector>();
            var uow = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            await using (uow.CreateScope())
            {
                var res = await detector.RunAsync(stoppingToken);
                detectionCount = res.Detections;
            }

            if (detectionCount > 0)
            {
                await notifier.NotifyAsync(detectionCount, DateTimeOffset.UtcNow,
                    stoppingToken);
            }
        }

        if (detectionCount > 0)
        {
            await buzzer.BuzzAsync(250, 1000, stoppingToken);
        }
    }
}