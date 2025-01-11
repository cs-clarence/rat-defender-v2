using Common.Utilities.Types;

namespace Common.Domain.Factories.Abstractions;

public interface IParameterlessSyncFactory<out TObject>
    : ISyncFactory<TObject, None>
{
    TObject ISyncFactory<TObject, None>.Create(None parameterObject)
    {
        return Create();
    }

    TObject Create();
}
