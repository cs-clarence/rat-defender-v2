namespace RatDefender.Application.Dtos;

public record RatDetectionDaySummaryDto(
    DateOnly Day,
    ulong Count
);