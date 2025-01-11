namespace Common.AspNetCore.Dtos.Responses;

public record DataResponse<TData>(TData Data, string Message = "success")
{
    public static implicit operator DataResponse<TData>(TData data)
    {
        return new(data);
    }
}

public record CursorPagedDataResponse<TData>(
    ICollection<TData> Data,
    ulong TotalCount,
    string? StartCursor = default,
    string? EndCursor = default,
    string Message = "success"
)
{
    public ulong PageSize => (ulong)Data.Count;
}

public record PagedDataResponse<TData>(
    ICollection<TData> Data,
    ulong TotalCount,
    string Message = "success"
)
{
    public ulong PageSize => (ulong)Data.Count;
}

public record OffsetPagedDataResponse<TData>(
    ICollection<TData> Data,
    ulong TotalCount,
    string Message = "success"
)
{
    public ulong PageSize => (ulong)Data.Count;
}
