namespace RatDefender.Domain.Configurations;

public class MockOptions
{
    public static readonly string DefaultKey =
        $"Mocks";

    public bool ThermalImager { get; init; } = false;
    public bool FoodDispenser { get; init; } = false;
    public bool Buzzer { get; init; } = false;
    public bool RatDetectionImageProcessor { get; init; } = false;
}