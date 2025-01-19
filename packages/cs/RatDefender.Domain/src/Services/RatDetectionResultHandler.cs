using RatDefender.Domain.Entities;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Domain.Services;

public class RatDetectionResultHandler(
    IRatDetectionRecordsService records,
    IBuzzer buzzer,
    IFoodDispenser dispenser,
    IDetectionNotifier notifier
) : IRatDetectionResultHandler
{
    public Task HandleAsync(DetectionResult result,
        CancellationToken stoppingToken = default)
    {
        if (!result.IsDetected) return Task.CompletedTask;

        var t1 = records.AddDetectionAsync(
            new RatDetection(
                result.DetectedAt,
                result.Detections
            ),
            stoppingToken
        );
        var t2 = buzzer.BuzzAsync(250, 1000, stoppingToken);
        var t3 = dispenser.DispenseAsync(1, stoppingToken);
        var t4 = notifier.NotifyAsync(result.Detections,
            result.DetectedAt, stoppingToken);

        stoppingToken.ThrowIfCancellationRequested();
        return Task.WhenAll(t1, t2, t3, t4);
    }
}