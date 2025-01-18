using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Infrastructure.Iot.Mocks;

public class MockDetectionNotifier(
    ILogger<MockDetectionNotifier> logger,
    ITaskQueueHandle tq)
    : IDetectionNotifier
{
    public Task NotifyAsync(ulong detectionCount = 0,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        return tq.EnqueueAsync(async () =>
        {
            await Task.Delay(1000, cancellationToken); // simulate delay
            var tz =
                TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            var now =
                TimeZoneInfo.ConvertTime(detectedAt ?? DateTimeOffset.UtcNow,
                    tz);
            logger.LogInformation(
                "NOTIFICATION: Detected {count} rat(s) at {time}",
                detectionCount, now);
        }, cancellationToken);
    }
}