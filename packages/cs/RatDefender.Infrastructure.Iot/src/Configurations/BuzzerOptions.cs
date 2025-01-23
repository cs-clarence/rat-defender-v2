using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record BuzzerOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(Buzzer).FullNameSection()!}";
    
    [Required]
    public ushort PwmChipNumber { get; init; } = 1;
    [Required]
    public ushort PwmChannel { get; init; } = 0;
    
    [Required]
    public ushort PwmFrequency { get; init; } = 50;
    
    [Required]
    public float PwmDutyPercent { get; init; } = 0.5f;
    
    [Required]
    public uint BuzzTone { get; init; } = 20000;
    
    [Required]
    public uint BuzzDurationMs { get; init; } = 1000; // 1 second
    
    [Required]
    public uint BuzzDelayMs { get; init; } = 30_000; // 30 seconds
}

[OptionsValidator]
public partial class
    BuzzerOptionsValidator : IValidateOptions<BuzzerOptions>;