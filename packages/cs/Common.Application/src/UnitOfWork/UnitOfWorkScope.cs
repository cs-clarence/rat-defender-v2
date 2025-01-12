using Common.Application.UnitOfWork.Abstractions;
using Common.Domain.Repositories.Abstractions;

namespace Common.Application.UnitOfWork;

public class UnitOfWorkScope : IUnitOfWorkScope
{
    private readonly IUnitOfWork _uow;

    public UnitOfWorkScope(IUnitOfWork uow,
        TrackingBehavior? trackingBehavior = null)
    {
        this._uow = uow;
        if (trackingBehavior is not null)
        {
            uow.Begin(trackingBehavior.Value);
        }
        else
        {
            uow.Begin();
        }
    }

    public void Dispose()
    {
        _uow.Commit();
    }

    public async ValueTask DisposeAsync()
    {
        await _uow.CommitAsync();
    }
}