namespace RatDefender.Domain.Services.Abstractions;

public interface IDetectionNotifier
{
    public Task NotifyAsync(ulong detectionCount = 1,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default);
}