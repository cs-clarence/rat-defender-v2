namespace RatDefender.Application.Dtos;

public record RatDetectionDto(
    int Id,
    DateTimeOffset DetectedAt,
    ulong Count
);