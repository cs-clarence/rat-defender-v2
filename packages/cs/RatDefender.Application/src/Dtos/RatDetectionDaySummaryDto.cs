namespace RatDefender.Application.Dtos;

public record RatDetectionDaySummaryDto(
    DateOnly Date,
    ulong Count
);