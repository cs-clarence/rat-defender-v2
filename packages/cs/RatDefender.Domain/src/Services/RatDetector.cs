using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Entities;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Domain.Services;

public record DetectionResult(
    ulong Detections = 0
);

public class RatDetector(
    IOptions<RatDetectorOptions> options,
    IRatDetectionImageProcessor imageProcessor,
    IThermalImager thermalImager,
    ILogger<RatDetector> logger,
    IFoodDispenser foodDispenser,
    IRatDetectionRecordsService records) : IRatDetector
{
    private static bool IsRatDetected(ThermalImagerReading reading,
        RatDetectorOptions options)
    {
        var width = reading.Image.GetLength(0);
        var length = reading.Image.GetLength(1);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < length; y++)
            {
                var pixel = reading.Image[x, y];

                if (pixel.DegreesCelsius > options.MaximumTemperatureCelsius
                    && pixel.DegreesCelsius < options.MinimumTemperatureCelsius)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<DetectionResult> RunAsync(
        CancellationToken stoppingToken = default)
    {
        var opt = options.Value;
        var results = await imageProcessor.ProcessImageAsync(stoppingToken);
        var count =
            results.Detections.Any(
                box => box.Confidence > opt.MinimumConfidence)
                ? 1u
                : 0u;

        if (count == 0)
        {
            return new DetectionResult(count);
        }


        var thermReading = await thermalImager.ReadImageAsync(stoppingToken);

        if (IsRatDetected(thermReading, opt) || !opt.UseThermalSensor)
        {
            var t = records.AddDetectionAsync(new RatDetection(
                DateTimeOffset.UtcNow,
                count
            ), stoppingToken);

            await Task.WhenAll(t,
                foodDispenser.DispenseAsync(count, stoppingToken));
            logger.LogInformation("Detected {count} rats", count);
        }


        return new DetectionResult(count);
    }
}