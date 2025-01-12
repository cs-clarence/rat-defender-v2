using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services;

namespace RatDefender.Domain.Configurations;

public record RatDetectorOptions
{
    public static readonly string DefaultKey = $"Slices:{typeof(RatDetector).FullNameSection()!}";

    [Range(0.0, 1.0)]
    public float MinimumConfidence { get; init; } = 0.5f;
    public float MinimumTemperatureCelsius { get; init; } = 30f;
    public float MaximumTemperatureCelsius { get; init; } = 40f;
}

[OptionsValidator]
public partial class
    RatDetectorOptionsValidator : IValidateOptions<RatDetectorOptions>;