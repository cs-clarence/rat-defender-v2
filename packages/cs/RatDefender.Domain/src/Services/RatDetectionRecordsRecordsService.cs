using RatDefender.Domain.Entities;
using RatDefender.Domain.Repositories.Abstractions;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Domain.ValueObjects;

namespace RatDefender.Domain.Services;

public class RatDetectionRecordsRecordsService(IRatDetectionRepository repo)
    : IRatDetectionRecordsService
{
    public async Task<RatDetection> RecordDetectionAsync(
        CancellationToken cancellationToken)
    {
        var dateTimeNow = DateTimeOffset.Now;

        var existingDetection =
            await repo.FindAsync(x => x.DetectedAt == dateTimeNow,
                cancellationToken);

        var detection = existingDetection ?? new RatDetection(dateTimeNow);

        if (existingDetection is not null)
        {
            detection.IncreaseCount();
        }

        repo.Add(detection);

        return detection;
    }

    public Task AddDetectionAsync(RatDetection detection,
        CancellationToken cancellationToken = default)
    {
        return repo.AddAsync(detection, cancellationToken);
    }

    public Task AddDetectionsAsync(IEnumerable<RatDetection> detections,
        CancellationToken cancellationToken = default)
    {
        return repo.AddManyAsync(detections, cancellationToken);
    }

    public Task<ICollection<RatDetection>> GetDetectionsAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var fromDate = from;
        var toDate = to;

        if (fromDate is not null && toDate is not null)
        {
            return repo.FindManyAsync(x =>
                    x.DetectedAt.CompareTo(fromDate.Value) >= 0 &&
                    x.DetectedAt.CompareTo(toDate.Value) <= 0,
                cancellationToken);
        }

        if (fromDate is not null)
        {
            return repo.FindManyAsync(
                x => x.DetectedAt.CompareTo(fromDate.Value) >= 0,
                cancellationToken);
        }

        if (toDate is not null)
        {
            return repo.FindManyAsync(
                x => x.DetectedAt.CompareTo(toDate.Value) <= 0,
                cancellationToken);
        }


        return repo.GetAllAsync(cancellationToken);
    }

    public async Task<ICollection<RatDetectionDaySummary>>
        GetDetectionsDailySummaryAsync(
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default)
    {
        var all = await GetDetectionsAsync(from, to, cancellationToken);

        var result = new Dictionary<DateOnly, ulong>();

        foreach (var detection in all)
        {
            var key = new DateOnly(detection.DetectedAt.Year,
                detection.DetectedAt.Month, detection.DetectedAt.Day);

            if (result.TryGetValue(key, out var count))
            {
                result[key] = count + detection.Count;
            }
            else
            {
                result.Add(key, detection.Count);
            }
        }

        return result.Select(kv => new RatDetectionDaySummary(kv.Key, kv.Value))
            .ToList();
    }

    public Task<ICollection<RatDetection>> GetDetectionsByDayAsync(DateOnly day,
        CancellationToken cancellationToken = default)
    {
        var start = day.ToDateTime(new TimeOnly(0, 0));
        var end = day.ToDateTime(new TimeOnly(23, 59, 59));
        var startDto = new DateTimeOffset(start, TimeSpan.Zero);
        var endDto = new DateTimeOffset(end, TimeSpan.Zero);

        return repo.FindManyAsync(
            x => x.DetectedAt.CompareTo(startDto) >= 0 &&
                 x.DetectedAt.CompareTo(endDto) <= 0,
            cancellationToken);
    }

    public Task<RatDetection?> FindDetectionByIdAsync(RatDetectionId id,
        CancellationToken cancellationToken = default)
    {
        return repo.FindByIdAsync(id, cancellationToken);
    }

    public Task<RatDetection> GetDetectionByIdAsync(RatDetectionId id,
        CancellationToken cancellationToken = default)
    {
        return repo.GetByIdAsync(id, cancellationToken);
    }
}