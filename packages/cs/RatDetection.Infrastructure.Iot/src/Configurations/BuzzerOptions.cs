namespace RatDetection.Infrastructure.Iot.Configurations;

public class BuzzerOptions
{
    public ushort BuzzerPwmChip { get; init; } = 0;
    public ushort BuzzerPwmChannel { get; init; } = 0;
    public ushort BuzzerPwmFrequency { get; init; } = 50;
    public float BuzzerPwmDutyPercent { get; init; } = 0.5f;
    public ushort BuzzerTone { get; init; } = 250;
    public ushort BuzzerDuration { get; init; } = 1000;
}