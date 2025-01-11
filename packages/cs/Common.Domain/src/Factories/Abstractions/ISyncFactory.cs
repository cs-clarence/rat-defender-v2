namespace Common.Domain.Factories.Abstractions;

public interface ISyncFactory<out TObject, in TParameters>
{
    TObject Create(TParameters parameterObject);
}
