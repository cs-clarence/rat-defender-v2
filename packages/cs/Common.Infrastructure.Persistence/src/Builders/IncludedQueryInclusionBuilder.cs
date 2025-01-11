using System.Linq.Expressions;
using Common.Domain.Repositories.Abstractions.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Common.Infrastructure.Persistence.Builders;

public class IncludedQueryInclusionBuilder<TEntity, TProperty>(
    IQueryable<TEntity> queryable) :
    QueryInclusionBuilder<TEntity>(queryable),
    IIncludedQueryInclusionBuilder<TEntity, TProperty>
    where TEntity : class
{
    private bool IsEnumerable =>
        Queryable is IIncludableQueryable<TEntity, IEnumerable<TProperty>>;

    private IIncludableQueryable<TEntity, IEnumerable<TProperty>> Enumerable =>
        (IIncludableQueryable<TEntity, IEnumerable<TProperty>>)Queryable;

    private IIncludableQueryable<TEntity, TProperty> NonEnumerable =>
        (IIncludableQueryable<TEntity, TProperty>)Queryable;

    public IIncludedQueryInclusionBuilder<
        TEntity,
        TIncluded
    > ThenInclude<TIncluded>(
        Expression<
            Func<TProperty, IEnumerable<TIncluded>>
        > navigationPropertySelector
    )
        where TIncluded : class
    {
        return new IncludedQueryInclusionBuilder<
            TEntity,
            TIncluded
        >(
            IsEnumerable
                ? Enumerable.ThenInclude(navigationPropertySelector)
                : NonEnumerable.ThenInclude(navigationPropertySelector)
        );
    }

    public IIncludedQueryInclusionBuilder<
        TEntity,
        TIncludedProperty
    > ThenInclude<TIncludedProperty>(
        Expression<
            Func<TProperty, TIncludedProperty>
        > navigationPropertySelector
    )
        where TIncludedProperty : class
    {
        return new IncludedQueryInclusionBuilder<TEntity, TIncludedProperty>(
            IsEnumerable
                ? Enumerable.ThenInclude(navigationPropertySelector)
                : NonEnumerable.ThenInclude(navigationPropertySelector)
        );
    }
}