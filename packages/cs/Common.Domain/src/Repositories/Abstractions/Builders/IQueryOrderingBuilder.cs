using System.Linq.Expressions;

namespace Common.Domain.Repositories.Abstractions.Builders;

public interface IQueryOrderingBuilder<TEntity>
{
    IOrderedQueryOrderingBuilder<TEntity> OrderBy<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    );

    IOrderedQueryOrderingBuilder<TEntity> OrderByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector
    );
}
