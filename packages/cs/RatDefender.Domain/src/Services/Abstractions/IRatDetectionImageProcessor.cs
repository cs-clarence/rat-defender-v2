namespace RatDefender.Domain.Services.Abstractions;

public record struct DetectionBoundingBox(
    ulong X,
    ulong Y,
    ulong Width,
    ulong Height,
    float Confidence,
    string Label);

public record struct ProcessResult(
    ICollection<DetectionBoundingBox> Detections);

public record struct ProcessOptions(
    float? MinimumConfidence = null,
    bool? ShowLabel = null,
    bool? ShowConfidence = null,
    bool? DetectRats = null
)
{
    public static ProcessOptions Default { get; } = new();
}

public interface IRatDetectionImageProcessor
{
    public Task<ProcessResult> ProcessImageAsync(
        CancellationToken cancellationToken = default);

    public Task<ProcessResult> ProcessImageAsync(ProcessOptions options,
        CancellationToken cancellationToken = default);
}