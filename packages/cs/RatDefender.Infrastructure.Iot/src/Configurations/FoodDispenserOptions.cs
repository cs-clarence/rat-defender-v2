using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public class FoodDispenserOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(FoodDispenser).FullNameSection()!}";

    [Range(0, 180)] public ushort DispenseAngle { get; init; } = 90;

    public uint RotationDelay { get; init; } = 1000;
    public uint DispenseDelay { get; init; } = 1000;

    public ushort ServoPwmChip { get; init; } = 0;
    public ushort ServoPwmChannel { get; init; } = 0;
    public ushort ServoPwmFrequency { get; init; } = 50;
    public float ServoPwmDutyPercent { get; init; } = 0.5f;
    public double ServoMaximumAngle { get; init; } = 180f;
    public double MinimumPulseWidthMicroseconds { get; init; } = 1000d;
    public double MaximumPulseWidthMicroseconds { get; init; } = 2000d;
}

[OptionsValidator]
public partial class
    FoodDispenserOptionsValidator : IValidateOptions<FoodDispenserOptions>;