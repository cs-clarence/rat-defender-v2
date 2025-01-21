using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Domain.Services;

public record DetectionResult(
    ulong Detections,
    bool IsTemperatureDetected,
    bool IgnoreTemperature,
    DateTimeOffset DetectedAt
)
{
    public bool IsDetected
        => Detections > 0 && (IgnoreTemperature || IsTemperatureDetected);
}

public class RatDetector(
    IOptions<RatDetectorOptions> options,
    IRatDetectionImageProcessor imageProcessor,
    IThermalImager thermalImager
) : IRatDetector
{
    private static bool IsRatTemperatureDetected(ThermalImagerReading reading,
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


    public async Task<DetectionResult> GetDetectionsAsync(
        CancellationToken stoppingToken = default)
    {
        var opt = options.Value;
        var thermReading = await thermalImager.ReadImageAsync(stoppingToken);
        var temperatureDetected = IsRatTemperatureDetected(thermReading, opt);

        var results = await imageProcessor.ProcessImageAsync(stoppingToken);
        var count =
            results.Detections.Any(
                box => box.Confidence >= opt.MinimumConfidence)
                ? 1u
                : 0u;

        stoppingToken.ThrowIfCancellationRequested();
        return new DetectionResult(count, temperatureDetected,
            !opt.UseThermalSensor, DateTimeOffset.UtcNow);
    }
}