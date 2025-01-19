namespace RatDefender.Domain.Services.Abstractions;

public interface IRatDetectionResultHandler
{
    public Task HandleAsync(DetectionResult result,
        CancellationToken stoppingToken = default);
}