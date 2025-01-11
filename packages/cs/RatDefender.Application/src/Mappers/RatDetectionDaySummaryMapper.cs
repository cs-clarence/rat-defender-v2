using RatDefender.Application.Dtos;
using RatDefender.Domain.ValueObjects;
using Riok.Mapperly.Abstractions;

namespace RatDefender.Application.Mappers;

[Mapper]
public static partial class RatDetectionDaySummaryMapper
{
    public static partial RatDetectionDaySummaryDto MapToDto(
        this RatDetectionDaySummary ratDetectionDaySummary);
    
    public static partial IEnumerable<RatDetectionDaySummaryDto> MapToDto(
        this IEnumerable<RatDetectionDaySummary> ratDetectionDaySummaries);
}