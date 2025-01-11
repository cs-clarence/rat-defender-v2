namespace Common.Domain.Factories.Abstractions;

public interface IFactory<TEntity, in TParameters>
    : ISyncFactory<TEntity, TParameters>,
        IAsyncFactory<TEntity, TParameters>
{
    Task<TEntity> IAsyncFactory<TEntity, TParameters>.CreateAsync(
        TParameters parameterObject
    )
    {
        return Task.FromResult(Create(parameterObject));
    }
}
