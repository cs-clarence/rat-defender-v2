using RatDefender.Application.Dtos;

namespace RatDefender.Application.Services.Abstractions;

public interface IRatDetectionService
{
    public Task<RatDetectionDto> RecordDetectionAsync(
        CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetectionDto>> GetDetectionsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetectionDaySummaryDto>>
        GetDetectionsDailySummaryAsync(
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetectionDto>> GetDetectionsByDayAsync(
        DateOnly day,
        CancellationToken cancellationToken = default);
    
    public Task<RatDetectionDto?> FindDetectionByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
    
    public Task<RatDetectionDto> GetDetectionByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}