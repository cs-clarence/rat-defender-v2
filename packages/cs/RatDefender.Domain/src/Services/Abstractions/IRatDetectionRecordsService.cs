using RatDefender.Domain.Entities;
using RatDefender.Domain.ValueObjects;

namespace RatDefender.Domain.Services.Abstractions;

public interface IRatDetectionRecordsService
{
    public Task<RatDetection> RecordDetectionAsync(
        CancellationToken cancellationToken = default);

    public Task AddDetectionAsync(RatDetection detection,
        CancellationToken cancellationToken = default);

    public Task AddDetectionsAsync(IEnumerable<RatDetection> detections,
        CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetection>> GetDetectionsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetectionDaySummary>>
        GetDetectionsDailySummaryAsync(
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default);

    public Task<ICollection<RatDetection>> GetDetectionsByDayAsync(
        DateOnly day,
        CancellationToken cancellationToken = default);

    public Task<RatDetection?> FindDetectionByIdAsync(
        RatDetectionId id,
        CancellationToken cancellationToken = default);

    public Task<RatDetection> GetDetectionByIdAsync(
        RatDetectionId id,
        CancellationToken cancellationToken = default);
    
    public Task DeleteDetectionAsync(RatDetectionId id,
        CancellationToken cancellationToken = default);

    public Task DeleteAllDetectionsAsync(
        CancellationToken cancellationToken = default);
}