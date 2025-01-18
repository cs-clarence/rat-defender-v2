using RatDefender.Domain.Entities.Abstractions;
using WrapperGen.Abstractions;

namespace RatDefender.Domain.Entities;

[Wrapper]
public readonly partial record struct RatDetectionId(int Value);

public class RatDetection(
    DateTimeOffset detectedAt,
    ulong count = 1) : IEntity<RatDetectionId>
{
    public RatDetectionId Id { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; } = detectedAt;
    public ulong Count { get; private set; } = count;

    public void IncreaseCount()
    {
        ++Count;
    }
}