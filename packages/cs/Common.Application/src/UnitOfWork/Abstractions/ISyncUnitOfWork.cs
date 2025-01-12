using Common.Domain.Repositories.Abstractions;

namespace Common.Application.UnitOfWork.Abstractions;

public interface ISyncUnitOfWork : IDisposable
{
    void Begin(TrackingBehavior trackingBehavior);

    void Begin()
    {
        Begin(TrackingBehavior.TrackAll);
    }
    void Commit();
    void Rollback();
    ulong SaveChanges();
}
