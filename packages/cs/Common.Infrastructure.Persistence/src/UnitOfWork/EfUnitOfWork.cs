using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using Common.Application.UnitOfWork.Abstractions;
using Common.Domain.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.Infrastructure.Persistence.UnitOfWork;

// TODO: not saving changes when exec ends
public class EfUnitOfWork(ActiveDbContextCollection dbContexts) : IUnitOfWork
{
    private IList<DbContext>? _dbContexts;
    private bool _singleContext;
    private DbContext? _firstContext;
    private IDbContextTransaction? _transaction;

    private readonly Stack<
        Dictionary<DbContext, QueryTrackingBehavior>
    > _queryTrackingBehaviorPreviousValueStack = new();

    [MemberNotNull(
        nameof(_dbContexts),
        nameof(_firstContext),
        nameof(_singleContext)
    )]
    private void ApplyDbContexts()
    {
        _dbContexts = dbContexts.GetAll().ToList();
        _singleContext = _dbContexts.Count == 1;
        _firstContext = _dbContexts.First();
    }

    public async Task ExecTrackingAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.TrackAll);
        await block(cancellationToken);
        await CommitAsync(cancellationToken);
    }

    public async Task<long> SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = 0L;
        foreach (var context in _dbContexts ?? Enumerable.Empty<DbContext>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!context.ChangeTracker.HasChanges())
                continue;

            result += await context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public void ExecTracking(Action block)
    {
        throw new NotImplementedException();
    }

    public long SaveChanges()
    {
        var result = 0L;
        foreach (var context in _dbContexts ?? Enumerable.Empty<DbContext>())
        {
            if (context.ChangeTracker.HasChanges())
            {
                result += context.SaveChanges();
            }
        }

        return result;
    }

    private async Task CommitAsync(
        CancellationToken cancellationToken = default
    )
    {
        await SaveChangesAsync(cancellationToken);

        if (_transaction != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _transaction.CommitAsync(cancellationToken);
        }

        RestorePreviousQueryTrackingBehavior();
    }

    private static QueryTrackingBehavior MapTrackingBehavior(
        TrackingBehavior trackingBehavior
    ) => trackingBehavior switch
    {
        TrackingBehavior.NoTracking => QueryTrackingBehavior.NoTracking,
        TrackingBehavior.TrackAll => QueryTrackingBehavior.TrackAll,
        TrackingBehavior.NoTrackingWithIdentityResolution =>
            QueryTrackingBehavior.NoTrackingWithIdentityResolution,
        _ => throw new ArgumentOutOfRangeException(
            nameof(trackingBehavior),
            trackingBehavior,
            null
        ),
    };

    private void RememberPreviousQueryTrackingBehavior()
    {
        var previousValue = new Dictionary<DbContext, QueryTrackingBehavior>();

        foreach (var context in _dbContexts ?? Enumerable.Empty<DbContext>())
        {
            previousValue.Add(
                context,
                context.ChangeTracker.QueryTrackingBehavior
            );
        }

        _queryTrackingBehaviorPreviousValueStack.Push(previousValue);
    }

    private void RestorePreviousQueryTrackingBehavior()
    {
        var previousValue = _queryTrackingBehaviorPreviousValueStack.Pop();

        foreach (var context in _dbContexts ?? Enumerable.Empty<DbContext>())
        {
            if (previousValue.TryGetValue(context, out var value))
            {
                context.ChangeTracker.QueryTrackingBehavior = value;
            }
        }
    }

    private void Begin(TrackingBehavior? trackingBehavior = null)
    {
        ApplyDbContexts();

        foreach (var context in _dbContexts)
        {
            if (Transaction.Current == null)
            {
                if (_transaction != null)
                {
                    context.Database.UseTransaction(
                        _transaction.GetDbTransaction()
                    );
                }
                else
                {
                    _transaction = context.Database.BeginTransaction();
                }
            }

            RememberPreviousQueryTrackingBehavior();
            if (trackingBehavior is not null)
            {
                context.ChangeTracker.QueryTrackingBehavior =
                    MapTrackingBehavior(trackingBehavior.Value);
            }
        }
    }

    private void Commit()
    {
        SaveChanges();

        _transaction?.Commit();
        RestorePreviousQueryTrackingBehavior();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    public async Task<T> ExecAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin();
        var result = await block(cancellationToken);
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin();
        var result = await block();
        await CommitAsync(cancellationToken);
        return result;
    }

    public async Task<T> ExecNoTrackingAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTracking);
        var result = await block(cancellationToken);
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecNoTrackingAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTracking);
        var result = await block();
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        var result = await block(cancellationToken);
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        var result = await block();
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecTrackingAsync<T>(
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.TrackAll);
        var result = await block(cancellationToken);
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task<T> ExecTrackingAsync<T>(
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.TrackAll);
        var result = await block();
        await CommitAsync(cancellationToken);

        return result;
    }

    public async Task ExecAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin();
        await block();
        await CommitAsync(cancellationToken);
    }

    public async Task ExecAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin();
        await block(cancellationToken);
        await CommitAsync(cancellationToken);
    }

    public async Task ExecNoTrackingAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTracking);
        await block();
        await CommitAsync(cancellationToken);
    }

    public async Task ExecNoTrackingAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTracking);
        await block(cancellationToken);
        await CommitAsync(cancellationToken);
    }

    public async Task ExecNoTrackingWithIdentityResolutionAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        await block();
        await CommitAsync(cancellationToken);
    }

    public async Task ExecNoTrackingWithIdentityResolutionAsync(
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        await block(cancellationToken);
        await CommitAsync(cancellationToken);
    }

    public async Task ExecTrackingAsync(
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        Begin(TrackingBehavior.TrackAll);
        await block();
        await CommitAsync(cancellationToken);
    }

    public T Exec<T>(Func<T> block)
    {
        Begin();
        var result = block();
        Commit();

        return result;
    }

    public void Exec(Action block)
    {
        Begin();
        block();
        Commit();
    }

    public T ExecNoTracking<T>(Func<T> block)
    {
        Begin(TrackingBehavior.NoTracking);
        var result = block();
        Commit();

        return result;
    }

    public void ExecNoTracking(Action block)
    {
        Begin(TrackingBehavior.NoTracking);
        block();
        Commit();
    }

    public T ExecNoTrackingWithIdentityResolution<T>(Func<T> block)
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        var result = block();
        Commit();

        return result;
    }

    public void ExecNoTrackingWithIdentityResolution(Action block)
    {
        Begin(TrackingBehavior.NoTrackingWithIdentityResolution);
        block();
        Commit();
    }

    public T ExecTracking<T>(Func<T> block)
    {
        Begin(TrackingBehavior.TrackAll);
        var result = block();
        Commit();

        return result;
    }
}