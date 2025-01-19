namespace RatDefender.Domain.Services.Abstractions;

public interface IRatDetector
{
    public Task<DetectionResult> GetDetectionsAsync(CancellationToken stoppingToken = default);
}