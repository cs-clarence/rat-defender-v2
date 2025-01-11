using System.Linq.Expressions;

namespace Common.Domain.Repositories.Abstractions.Builders;

public interface IOrderedQueryOrderingBuilder<TEntity>
    : IQueryOrderingBuilder<TEntity>
{
    IOrderedQueryOrderingBuilder<TEntity> ThenBy<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    );

    IOrderedQueryOrderingBuilder<TEntity> ThenByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    );
}
