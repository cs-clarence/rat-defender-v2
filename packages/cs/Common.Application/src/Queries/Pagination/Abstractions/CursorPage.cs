namespace Common.Application.Queries.Pagination.Abstractions;

public record CursorPage<T>(
    ICollection<T> Items,
    ulong TotalCount,
    string? StartCursor = default,
    string? EndCursor = default
) : Page<T>(Items, TotalCount);
