using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using DebounceThrottle;
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
    IOptions<RatDetectorOptions> options
) : BackgroundService, IDisposable
{
    private readonly ThrottleDispatcher? _handlerThrottler =
        options.Value.MinimumTimeBetweenDetectionsSeconds is not null
            ? new(TimeSpan.FromSeconds(options.Value
                .MinimumTimeBetweenDetectionsSeconds.Value))
            : null;
    
    private bool _isPreviousDetected = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cts =
            CancellationTokenSource.CreateLinkedTokenSource([stoppingToken]);

        // allow startup to complete
        await Task.Delay(1000, cts.Token);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10, cts.Token);
                await RunAsync(cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // expected, service is stopping
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while running RatDetectionBackgroundService");
            await Task.Delay(1000, cts.Token);
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        using var scope = provider.CreateScope();
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
                if (_handlerThrottler is not null)
                {
                    await _handlerThrottler.ThrottleAsync(async () =>
                    {
                        logger.LogDebug("Running handler");
                        await handler.HandleAsync(res, stoppingToken);
                    }, stoppingToken);
                }
                else
                {
                    await handler.HandleAsync(res, stoppingToken);
                }
            }
        }
        _isPreviousDetected = res.IsDetected;
    }

    void IDisposable.Dispose()
    {
        _handlerThrottler?.Dispose();
    }
}