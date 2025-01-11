using Common.Application.Queries.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Common.AspNetCore.Queries;

public record CursorPaginationParams(
    [FromQuery(Name = "first")] ulong? First = default,
    [FromQuery(Name = "after")] string? After = default,
    [FromQuery(Name = "last")] ulong? Last = default,
    [FromQuery(Name = "before")] string? Before = default,
    [FromQuery(Name = "sort-by")] string[]? SortBy = default
)
{
    public static CursorPaginationParams None { get; } = new();

    private static bool IsValidFieldName(string s)
    {
        return s.All(char.IsLetterOrDigit);
    }

    private static Sorting ParseSorting(string s)
    {
        var parts = s.Split(':');

        if (parts.Length == 1)
        {
            var field = parts[0];

            if (!IsValidFieldName(field))
            {
                throw new ArgumentException(
                    "Invalid field name. Valid field names are alphanumeric."
                );
            }

            return new Sorting(field, SortOrder.Asc);
        }

        if (parts.Length == 2)
        {
            return new Sorting(
                parts[0],
                parts[1].ToLowerInvariant() switch
                {
                    "asc" => SortOrder.Asc,
                    "desc" => SortOrder.Desc,
                    _ => throw new ArgumentException(
                        "Invalid sort order. Valid values: 'asc' or 'desc'"
                    ),
                }
            );
        }

        throw new ArgumentException(
            "Invalid sorting parameter syntax. Valid syntax: 'field[:asc|desc]'. Examples: 'name', 'name:asc', 'name:desc'"
        );
    }

    public void ApplyTo(CursorPaginationQuery q)
    {
        q.First = First;
        q.After = After;
        q.Last = Last;
        q.Before = Before;
        q.SortBy = SortBy?.Select(ParseSorting);
    }
}
