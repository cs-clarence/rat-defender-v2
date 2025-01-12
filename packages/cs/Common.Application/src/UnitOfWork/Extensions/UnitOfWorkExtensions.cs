using Common.Application.UnitOfWork.Abstractions;
using Common.Domain.Repositories.Abstractions;

namespace Common.Application.UnitOfWork.Extensions;

public static class UnitOfWorkExtensions
{
    private static async Task ExecWithTrackingBehaviorAsync(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                await uow.BeginAsync(trackingBehavior.Value, cancellationToken);
            }
            else
            {
                await uow.BeginAsync(cancellationToken);
            }

            await block(cancellationToken);
            await uow.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task ExecWithTrackingBehaviorAsync(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Func<Task> block,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                await uow.BeginAsync(trackingBehavior.Value, cancellationToken);
            }
            else
            {
                await uow.BeginAsync(cancellationToken);
            }

            await block();
            await uow.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<T> ExecWithTrackingBehaviorAsync<T>(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                await uow.BeginAsync(trackingBehavior.Value, cancellationToken);
            }
            else
            {
                await uow.BeginAsync(cancellationToken);
            }

            var result = await block(cancellationToken);
            await uow.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<T> ExecWithTrackingBehaviorAsync<T>(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Func<Task<T>> block,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                await uow.BeginAsync(trackingBehavior.Value, cancellationToken);
            }
            else
            {
                await uow.BeginAsync(cancellationToken);
            }

            var result = await block();
            await uow.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public static Task<T> ExecAsync<T>(
        this IUnitOfWork uow,
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, null, block,
            cancellationToken);
    }

    public static Task<T> ExecAsync<T>(
        this IUnitOfWork uow,
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, null, block,
            cancellationToken);
    }

    public static Task<T> ExecNoTrackingAsync<T>(
        this IUnitOfWork uow,
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.NoTracking,
            block, cancellationToken);
    }

    public static Task<T> ExecNoTrackingAsync<T>(
        this IUnitOfWork uow,
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.NoTracking,
            block, cancellationToken);
    }

    public static Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        this IUnitOfWork uow,
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution,
            block, cancellationToken);
    }

    public static Task<T> ExecNoTrackingWithIdentityResolutionAsync<T>(
        this IUnitOfWork uow,
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution,
            block, cancellationToken);
    }

    public static Task<T> ExecTrackingAsync<T>(
        this IUnitOfWork uow,
        Func<CancellationToken, Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.TrackAll,
            block, cancellationToken);
    }

    public static Task<T> ExecTrackingAsync<T>(
        this IUnitOfWork uow,
        Func<Task<T>> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.TrackAll,
            block, cancellationToken);
    }

    public static Task ExecAsync(
        this IUnitOfWork uow,
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, null, block,
            cancellationToken);
    }

    public static Task ExecAsync(
        this IUnitOfWork uow,
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, null, block,
            cancellationToken);
    }

    public static Task ExecNoTrackingAsync(
        this IUnitOfWork uow,
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.NoTracking,
            block, cancellationToken);
    }

    public static Task ExecNoTrackingAsync(
        this IUnitOfWork uow,
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.NoTracking,
            block, cancellationToken);
    }

    public static Task ExecNoTrackingWithIdentityResolutionAsync(
        this IUnitOfWork uow,
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution,
            block, cancellationToken);
    }

    public static Task ExecNoTrackingWithIdentityResolutionAsync(
        this IUnitOfWork uow,
        Func<CancellationToken, Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution,
            block, cancellationToken);
    }

    public static Task ExecTrackingAsync(
        this IUnitOfWork uow,
        Func<Task> block,
        CancellationToken cancellationToken = default
    )
    {
        return ExecWithTrackingBehaviorAsync(uow, TrackingBehavior.TrackAll,
            block, cancellationToken);
    }


    private static void ExecWithTrackingBehavior(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Action block)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                uow.Begin(trackingBehavior.Value);
            }
            else
            {
                uow.Begin();
            }

            block();
            uow.Commit();
        }
        catch (Exception)
        {
            uow.Rollback();
            throw;
        }
    }

    private static T ExecWithTrackingBehavior<T>(
        this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior,
        Func<T> block)
    {
        try
        {
            if (trackingBehavior is not null)
            {
                uow.Begin(trackingBehavior.Value);
            }
            else
            {
                uow.Begin();
            }

            var result = block();
            uow.Commit();
            return result;
        }
        catch (Exception)
        {
            uow.Rollback();
            throw;
        }
    }

    public static T Exec<T>(
        this IUnitOfWork uow,
        Func<T> block)
    {
        return ExecWithTrackingBehavior(uow, null, block);
    }

    public static void Exec(
        this IUnitOfWork uow,
        Action block)
    {
        ExecWithTrackingBehavior(uow, null, block);
    }

    public static T ExecNoTracking<T>(
        this IUnitOfWork uow,
        Func<T> block)
    {
        return ExecWithTrackingBehavior(uow, TrackingBehavior.NoTracking,
            block);
    }

    public static void ExecNoTracking(
        this IUnitOfWork uow,
        Action block)
    {
        ExecWithTrackingBehavior(uow, TrackingBehavior.NoTracking, block);
    }

    public static T ExecNoTrackingWithIdentityResolution<T>(
        this IUnitOfWork uow,
        Func<T> block)
    {
        return ExecWithTrackingBehavior(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution, block);
    }

    public static void ExecNoTrackingWithIdentityResolution(
        this IUnitOfWork uow,
        Action block)
    {
        ExecWithTrackingBehavior(uow,
            TrackingBehavior.NoTrackingWithIdentityResolution, block);
    }

    public static T ExecTracking<T>(
        this IUnitOfWork uow,
        Func<T> block)
    {
        return ExecWithTrackingBehavior(uow, TrackingBehavior.TrackAll, block);
    }

    public static IUnitOfWorkScope CreateScope(this IUnitOfWork uow,
        TrackingBehavior? trackingBehavior = null)
    {
        return new UnitOfWorkScope(uow, trackingBehavior);
    }
}