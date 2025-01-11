using System.Linq.Expressions;
using Common.Domain.Repositories.Abstractions.Builders;

namespace Common.Infrastructure.Persistence.Builders;

public class QueryOrderingBuilder<TEntity>(IQueryable<TEntity> queryable)
    : IQueryOrderingBuilder<TEntity>
{
    public IOrderedQueryOrderingBuilder<TEntity> OrderBy<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    )
    {
        return new OrderedQueryOrderingBuilder<TEntity>(
            queryable.OrderBy(propertySelector)
        );
    }

    public IOrderedQueryOrderingBuilder<TEntity> OrderByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    )
    {
        return new OrderedQueryOrderingBuilder<TEntity>(
            queryable.OrderByDescending(propertySelector)
        );
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return queryable;
    }
}
