using System.Linq.Expressions;
using Common.Domain.Repositories.Abstractions.Builders;

namespace Common.Infrastructure.Persistence.Builders;

public class OrderedQueryOrderingBuilder<TEntity>(
    IOrderedQueryable<TEntity> queryable
)
    : QueryOrderingBuilder<TEntity>(queryable),
        IOrderedQueryOrderingBuilder<TEntity>
{
    public IOrderedQueryOrderingBuilder<TEntity> ThenBy<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    )
    {
        return new OrderedQueryOrderingBuilder<TEntity>(
            queryable.ThenBy(propertySelector)
        );
    }

    public IOrderedQueryOrderingBuilder<TEntity> ThenByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    )
    {
        return new OrderedQueryOrderingBuilder<TEntity>(
            queryable.ThenByDescending(propertySelector)
        );
    }
}
