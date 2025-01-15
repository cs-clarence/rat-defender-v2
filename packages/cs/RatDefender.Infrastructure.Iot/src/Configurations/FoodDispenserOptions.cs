using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record FoodDispenserOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(FoodDispenser).FullNameSection()!}";

    [Range(0, 180)] public ushort ServoDispenseAngle { get; init; } = 90;

    public uint ServoRotationDelay { get; init; } = 1000;
    public uint DispenseDelay { get; init; } = 1000;
    public ushort PwmChipNumber { get; init; } = 2;
    public ushort PwmChannel { get; init; } = 0;
    public ushort PwmFrequency { get; init; } = 50;
    public float PwmDutyPercent { get; init; } = 0.5f;
    public ushort ServoMaximumAngle { get; init; } = 180;
    public double ServoMinimumPulseWidthMicroseconds { get; init; } = 1000d;
    public double ServoMaximumPulseWidthMicroseconds { get; init; } = 2000d;
}

[OptionsValidator]
public partial class
    FoodDispenserOptionsValidator : IValidateOptions<FoodDispenserOptions>;