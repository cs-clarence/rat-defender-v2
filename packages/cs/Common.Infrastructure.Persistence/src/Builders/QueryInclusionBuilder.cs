using System.Linq.Expressions;
using Common.Domain.Repositories.Abstractions.Builders;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Builders;

public class QueryInclusionBuilder<TEntity>(IQueryable<TEntity> queryable)
    : IQueryInclusionBuilder<TEntity>
    where TEntity : class
{
    protected IQueryable<TEntity> Queryable => queryable;

    public IIncludedQueryInclusionBuilder<
        TEntity,
        TProperty
    > Include<TProperty>(
        Expression<Func<TEntity, TProperty>> navigationPropertySelector
    )
    {
        return new IncludedQueryInclusionBuilder<TEntity, TProperty>(
            Queryable.Include(navigationPropertySelector)
        );
    }

    public IIncludedQueryInclusionBuilder<TEntity, TIncluded>
        Include<TIncluded>(
            Expression<Func<TEntity, IEnumerable<TIncluded>>>
                navigationPropertySelector)
    {
        return new IncludedQueryInclusionBuilder<TEntity, TIncluded>(
            Queryable.Include(navigationPropertySelector)
        );
    }

    public IQueryInclusionBuilder<TEntity> Include(string navigationPath)
    {
        return new QueryInclusionBuilder<TEntity>(
            Queryable.Include(navigationPath)
        );
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return Queryable;
    }
}