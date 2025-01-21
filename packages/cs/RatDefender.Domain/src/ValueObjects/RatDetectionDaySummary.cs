namespace RatDefender.Domain.ValueObjects;

public record RatDetectionDaySummary(DateOnly Date, ulong Count);