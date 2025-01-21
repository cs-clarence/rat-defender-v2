using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services;

namespace RatDefender.Domain.Configurations;

public record RatDetectorOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(RatDetector).FullNameSection()!}";

    [Range(0.0, 1.0)] 
    public float MinimumConfidence { get; init; } = 0.5f;
    public float MinimumTemperatureCelsius { get; init; } = 35f;
    public float MaximumTemperatureCelsius { get; init; } = 40f;
    public bool UseThermalSensor { get; init; } = false;
    public uint? MinimumTimeBetweenDetectionsSeconds { get; init; } = 5;
    public bool DetectThermalBeforeObjectDetection { get; init; } = true;
}

[OptionsValidator]
public partial class
    RatDetectorOptionsValidator : IValidateOptions<RatDetectorOptions>;