using Common.Application.Queries.Pagination.Abstractions;
using Common.AspNetCore.Dtos.Responses;

namespace Common.AspNetCore.Responses;

public static class Responses
{
    public static SuccessResponse Success(string message = "Success")
    {
        return new SuccessResponse(message);
    }

    public static MessageResponse Message(string message)
    {
        return new MessageResponse(message);
    }

    public static DataResponse<TData> Data<TData>(
        TData data,
        string message = "success"
    )
    {
        return new DataResponse<TData>(data, message);
    }

    public static CursorPagedDataResponse<TData> CursorPagedData<TData>(
        ICollection<TData> data,
        ulong totalCount,
        string? startCursor = default,
        string? endCursor = default,
        string message = "success"
    )
    {
        return new CursorPagedDataResponse<TData>(
            data,
            totalCount,
            startCursor,
            endCursor,
            message
        );
    }

    public static CursorPagedDataResponse<TData> CursorPagedData<TData>(
        CursorPage<TData> page,
        string message = "success"
    )
    {
        return new CursorPagedDataResponse<TData>(
            page.Items,
            page.TotalCount,
            page.StartCursor,
            page.EndCursor,
            message
        );
    }

    public static OffsetPagedDataResponse<TData> OffsetPagedData<TData>(
        ICollection<TData> data,
        ulong totalCount,
        string message = "success"
    )
    {
        return new OffsetPagedDataResponse<TData>(data, totalCount, message);
    }

    public static OffsetPagedDataResponse<TData> OffsetPagedData<TData>(
        OffsetPage<TData> page,
        string message = "success"
    )
    {
        return new OffsetPagedDataResponse<TData>(
            page.Items,
            page.TotalCount,
            message
        );
    }

    public static PagedDataResponse<TData> PagedData<TData>(
        ICollection<TData> data,
        ulong totalCount,
        string message = "success"
    )
    {
        return new PagedDataResponse<TData>(data, totalCount, message);
    }

    public static PagedDataResponse<TData> PagedData<TData>(
        Page<TData> page,
        string message = "success"
    )
    {
        return new PagedDataResponse<TData>(
            page.Items,
            page.TotalCount,
            message
        );
    }
}
