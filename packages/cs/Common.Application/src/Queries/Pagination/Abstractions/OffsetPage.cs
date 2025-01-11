namespace Common.Application.Queries.Pagination.Abstractions;

public record OffsetPage<T>(ulong TotalCount, ICollection<T> Items)
    : Page<T>(Items, TotalCount);
