namespace RatDefender.Domain.Services.Abstractions;

public interface IRatDetector
{
    public Task<DetectionResult> RunAsync(CancellationToken stoppingToken = default);
}