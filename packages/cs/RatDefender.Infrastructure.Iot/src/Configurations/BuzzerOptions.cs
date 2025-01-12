using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public class BuzzerOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(Buzzer).FullNameSection()!}";
    
    [Required]
    public ushort BuzzerPwmChip { get; init; } = 1;
    
    [Required]
    public ushort BuzzerPwmChannel { get; init; } = 0;
    
    [Required]
    public ushort BuzzerPwmFrequency { get; init; } = 50;
    
    [Required]
    public float BuzzerPwmDutyPercent { get; init; } = 0.5f;
    
    [Required]
    public ushort BuzzerTone { get; init; } = 250;
    
    [Required]
    public ushort BuzzerDurationMs { get; init; } = 1000; // 1 second
    
    [Required]
    public ulong BuzzerDelayMs { get; init; } = 30_000; // 30 seconds
}

[OptionsValidator]
public partial class
    BuzzerOptionsValidator : IValidateOptions<BuzzerOptions>;