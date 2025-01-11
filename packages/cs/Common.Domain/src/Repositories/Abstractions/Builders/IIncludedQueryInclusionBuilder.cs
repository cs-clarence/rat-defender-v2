using System.Linq.Expressions;

namespace Common.Domain.Repositories.Abstractions.Builders;

public interface IIncludedQueryInclusionBuilder<TEntity, TPreviousIncluded>
    : IQueryInclusionBuilder<TEntity>
    where TEntity : class
{
    IIncludedQueryInclusionBuilder<
        TEntity, TIncluded
    > ThenInclude<TIncluded>(
        Expression<
            Func<TPreviousIncluded, IEnumerable<TIncluded>>
        > navigationPropertySelector
    )
        where TIncluded : class;

    IIncludedQueryInclusionBuilder<TEntity, TIncluded> ThenInclude<TIncluded>(
        Expression<
            Func<TPreviousIncluded, TIncluded>
        > navigationPropertySelector
    )
        where TIncluded : class;
}