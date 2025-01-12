using Common.Domain.Repositories.Abstractions;

namespace Common.Application.UnitOfWork.Abstractions;

public interface IAsyncUnitOfWork : IAsyncDisposable
{
    Task BeginAsync(CancellationToken cancellationToken = default)
    {
        return BeginAsync(TrackingBehavior.TrackAll, cancellationToken);
    }

    Task BeginAsync(TrackingBehavior trackingBehavior,
        CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task<ulong> SaveChangesAsync(CancellationToken cancellationToken = default);
}