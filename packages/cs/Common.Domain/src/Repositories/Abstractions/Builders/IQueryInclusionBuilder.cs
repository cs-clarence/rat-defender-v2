using System.Linq.Expressions;

namespace Common.Domain.Repositories.Abstractions.Builders;

public interface IQueryInclusionBuilder<TEntity>
    where TEntity : class
{
    IIncludedQueryInclusionBuilder<TEntity, TIncluded> Include<TIncluded>(
        Expression<Func<TEntity, TIncluded>> navigationPropertySelector
    );

    IIncludedQueryInclusionBuilder<TEntity, TIncluded> Include<
        TIncluded>(
        Expression<Func<TEntity, IEnumerable<TIncluded>>>
            navigationPropertySelector
    );

    IQueryInclusionBuilder<TEntity> Include(string navigationPath);
}