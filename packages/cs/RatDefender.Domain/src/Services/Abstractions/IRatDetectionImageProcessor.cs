namespace RatDefender.Domain.Services.Abstractions;

public record struct DetectionBox(int X, int Y, int Width, int Height, float Confidence, string Label);

public record struct ProcessResult(ICollection<DetectionBox> Detections);

public interface IRatDetectionImageProcessor
{
    public Task<ProcessResult> ProcessImageAsync(CancellationToken cancellationToken = default);
}