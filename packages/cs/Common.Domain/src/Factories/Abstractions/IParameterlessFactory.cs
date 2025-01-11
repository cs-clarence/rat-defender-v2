namespace Common.Domain.Factories.Abstractions;

public interface IParameterlessFactory<TObject>
    : IParameterlessAsyncFactory<TObject>,
        IParameterlessSyncFactory<TObject>
{
    Task<TObject> IParameterlessAsyncFactory<TObject>.CreateAsync()
    {
        return Task.FromResult(Create());
    }
}
