using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Infrastructure.ObjectDetection.Mocks;

public class MockRatDetectionImageProcessor(
    ILogger<RatDetectionImageProcessor> logger,
    ImageHolder holder) : IRatDetectionImageProcessor
{
    private readonly Random _random = new();
    private readonly ulong[] _randAccepted = [0, 1, 23, 44, 59, 23];

    public async Task<ProcessResult> ProcessImageAsync(
        CancellationToken ct = default)
    {
        // 200 ms delay to simulate processing time
        await Task.Delay(5000, ct);
        var result = _random.NextInt64(0, 100);
        // simulate low detection rate
        var count = _randAccepted.Contains((ulong)result) ? 1ul : 0ul;
        // logger.LogProcessedImage(count);
        holder.SetImageBuffer([], ".jpg");

        if (count > 0)
        {
            logger.LogInformation("Processed image with {count} detections",
                count);
            return new ProcessResult([
                new DetectionBoundingBox(0, 0, 100, 100, 0.75f, "rat"),
                new DetectionBoundingBox(0, 0, 100, 100, 0.80f, "rat")
            ]);
        }
        else
        {
            return new ProcessResult([
                new DetectionBoundingBox(0, 0, 100, 100, 0.35f, "rat"),
                new DetectionBoundingBox(0, 0, 100, 100, 0.25f, "rat")
            ]);
        }
    }
}