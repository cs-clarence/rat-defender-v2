namespace Common.Application.Queries.Abstractions;

public abstract record OffsetPaginationQuery(
    ulong? Offset = default,
    ulong? Limit = default,
    string? SortBy = default,
    SortOrder? SortOrder = default,
    string[]? Include = default
)
{
    public ulong? Offset { get; set; } = Offset;
    public ulong? Limit { get; set; } = Limit;
    public string? SortBy { get; set; } = SortBy;
    public SortOrder? SortOrder { get; set; } = SortOrder;
    public string[]? Include { get; set; } = Include;
}
