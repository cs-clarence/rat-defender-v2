namespace RatDefender.Domain.Configurations;

public class ServiceRegistrationOptions
{
    public static readonly string DefaultKey =
        $"ServiceRegistration";

    public bool MockThermalImager { get; init; } = false;
    public bool MockFoodDispenser { get; init; } = false;
    public bool MockBuzzer { get; init; } = false;
    public bool MockRatDetectionImageProcessor { get; init; } = false;
    public bool MockDetectionNotifier { get; init; } = false;
    public bool UseSerialAdapter { get; init; } = true;
}