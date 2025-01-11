namespace Common.Application.Queries.Abstractions;

public record Sorting(string By, SortOrder Order);

public abstract record CursorPaginationQuery(
    string? After = default,
    string? Before = default,
    ulong? First = default,
    ulong? Last = default,
    IEnumerable<Sorting>? SortBy = default
)
{
    public string? After { get; set; } = After;
    public string? Before { get; set; } = Before;
    public ulong? First { get; set; } = First;
    public ulong? Last { get; set; } = Last;
    public IEnumerable<Sorting>? SortBy { get; set; } = SortBy;
}
