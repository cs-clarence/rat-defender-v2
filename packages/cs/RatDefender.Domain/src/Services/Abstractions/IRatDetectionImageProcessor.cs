namespace RatDefender.Domain.Services.Abstractions;

public record struct DetectionBoundingBox(ulong X, ulong Y, ulong Width, ulong Height, float Confidence, string Label);

public record struct ProcessResult(ICollection<DetectionBoundingBox> Detections);

public interface IRatDetectionImageProcessor
{
    public Task<ProcessResult> ProcessImageAsync(CancellationToken cancellationToken = default);
}