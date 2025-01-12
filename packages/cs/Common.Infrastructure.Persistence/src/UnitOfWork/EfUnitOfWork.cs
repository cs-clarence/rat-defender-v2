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

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        Begin(TrackingBehavior.TrackAll);
        return Task.CompletedTask;
    }

    public Task BeginAsync(TrackingBehavior trackingBehavior,
        CancellationToken cancellationToken = default)
    {
        Begin(trackingBehavior);
        return Task.CompletedTask;
    }

    Task IAsyncUnitOfWork.CommitAsync(CancellationToken cancellationToken)
    {
        return CommitAsync(cancellationToken);
    }

    public  Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        _transaction = null;
        return Task.CompletedTask;
    }

    public async Task<ulong> SaveChangesAsync(
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

        return (ulong)result;
    }

    public void Rollback()
    {
        var tx = _transaction;
        
        if (tx is not null)
        {
            tx.Rollback();
        }
        
        _transaction = null;
    }

    public ulong SaveChanges()
    {
        var result = 0L;
        foreach (var context in _dbContexts ?? Enumerable.Empty<DbContext>())
        {
            if (context.ChangeTracker.HasChanges())
            {
                result += context.SaveChanges();
            }
        }

        return (ulong)result;
    }

    void ISyncUnitOfWork.Begin(TrackingBehavior trackingBehavior)
    {
        Begin(trackingBehavior);
    }

    public void Begin()
    {
        Begin(TrackingBehavior.TrackAll);
    }

    void ISyncUnitOfWork.Commit()
    {
        Commit();
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

    private void Begin(TrackingBehavior trackingBehavior)
    {
        ApplyDbContexts();

        foreach (var context in _dbContexts)
        {
            if (Transaction.Current == null)
            {
                if (_transaction is not null)
                {
                    context.Database.UseTransaction(_transaction.GetDbTransaction());
                }
                else
                {
                    _transaction = context.Database.BeginTransaction();
                }
            }

            RememberPreviousQueryTrackingBehavior();
            context.ChangeTracker.QueryTrackingBehavior =
                MapTrackingBehavior(trackingBehavior);
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
}