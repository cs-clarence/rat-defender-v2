using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.HostedServices;

public class RatDetectionBackgroundService(
    ILogger<RatDetectionBackgroundService> logger,
    IServiceProvider provider,
    IRatDetector detector,
    IOptions<RatDetectorOptions> options,
    IRatDetectionImageProcessor processor
) : BackgroundService
{
    private DateTimeOffset _lastDetection = DateTimeOffset.UtcNow;

    private bool _isPreviousDetected;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cts =
            CancellationTokenSource.CreateLinkedTokenSource([stoppingToken]);

        // allow startup to complete
        await Task.Delay(1000, cts.Token);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(10, cts.Token);
                await RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected, service is stopping
                logger.LogInformation("Stopping RatDetectionBackgroundService");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "An error occurred while running RatDetectionBackgroundService");
            }
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        using var scope = provider.CreateScope();


        // If the minimum time between detections is set and the last detection
        // was more than the minimum time ago, then we should not detect a rat
        // and only update the image
        var minimumTimeBetweenDetectionsSeconds =
            options.Value.MinimumTimeBetweenDetectionsSeconds;
        if (minimumTimeBetweenDetectionsSeconds is not null &&
            _lastDetection.AddSeconds(minimumTimeBetweenDetectionsSeconds
                .Value) > DateTimeOffset.UtcNow)
        {
            await processor.ProcessImageAsync(new ProcessOptions
            {
                DetectRats = false,
            }, stoppingToken);
            return;
        }

        var res = await detector.GetDetectionsAsync(stoppingToken);

        if (res.IsDetected && !_isPreviousDetected)
        {
            var uow = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();
            var handler = scope.ServiceProvider
                .GetRequiredService<IRatDetectionResultHandler>();
            logger.LogDebug("Rat detected");
            await using (uow.CreateScope())
            {
                _lastDetection = DateTimeOffset.UtcNow;
                await handler.HandleAsync(res, stoppingToken);
            }
        }

        _isPreviousDetected = res.IsDetected;
    }
}