namespace Common.Application.Queries.Pagination.Abstractions;

public abstract record Page<T>(ICollection<T> Items, ulong TotalCount)
{
    public ulong PageSize => (ulong)Items.Count;
}
