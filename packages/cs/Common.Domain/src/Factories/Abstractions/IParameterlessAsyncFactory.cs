using Common.Utilities.Types;

namespace Common.Domain.Factories.Abstractions;

public interface IParameterlessAsyncFactory<TObject>
    : IAsyncFactory<TObject, None>
{
    Task<TObject> IAsyncFactory<TObject, None>.CreateAsync(None parameterObject)
    {
        return CreateAsync();
    }

    Task<TObject> CreateAsync();
}
