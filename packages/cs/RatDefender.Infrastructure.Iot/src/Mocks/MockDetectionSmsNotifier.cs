using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Infrastructure.Iot.Mocks;

public class MockDetectionSmsNotifier(ILogger<MockDetectionSmsNotifier> logger)
    : IDetectionNotifier
{
    public async Task NotifyAsync(ulong detectionCount = 0,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken); // simulate delay
        logger.LogInformation("NOTIFICATION: Detected {count} rats at {time}",
            detectionCount, detectedAt);
    }
}