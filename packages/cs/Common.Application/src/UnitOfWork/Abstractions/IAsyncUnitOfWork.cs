namespace Common.Application.UnitOfWork.Abstractions;

public interface IAsyncUnitOfWork : IAsyncDisposable
{
    Task<T> ExecAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecNoTrackingAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecNoTrackingAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecTrackingAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecTrackingAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    );

    Task ExecAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecNoTrackingAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecNoTrackingAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecNoTrackingWithIdentityResolutionAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecNoTrackingWithIdentityResolutionAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecTrackingAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    );

    Task ExecTrackingAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    );

    Task<long> SaveChangesAsync(CancellationToken cancellationToken = default);
}
