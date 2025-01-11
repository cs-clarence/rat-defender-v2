namespace Common.Domain.Factories.Abstractions;

public interface IAsyncFactory<TObject, in TParameters>
{
    Task<TObject> CreateAsync(TParameters parameterObject);
}
