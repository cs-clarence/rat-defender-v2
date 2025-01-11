namespace RatDefender.Domain.ValueObjects;

public record RatDetectionDaySummary(DateOnly Day, ulong Count);