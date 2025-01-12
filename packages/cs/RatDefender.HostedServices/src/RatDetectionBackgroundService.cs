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
    IBuzzer buzzer
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunAsync(stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while running RatDetectionBackgroundService");
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10, stoppingToken);
        using var scope = provider.CreateScope();
        var detector = scope.ServiceProvider
            .GetRequiredService<IRatDetector>();
        var uow = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        await using var uowScope = uow.CreateScope();
        var res = await detector.RunAsync(stoppingToken);
        
        if (res.Detections > 0)
        {
            await buzzer.BuzzAsync(250, 1000, stoppingToken);
        }
    }
}