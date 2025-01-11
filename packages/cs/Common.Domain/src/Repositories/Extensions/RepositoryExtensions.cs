using Common.Domain.Entities.Abstractions;
using Common.Domain.Repositories.Abstractions;

namespace Common.Domain.Repositories.Extensions;

public static class RepositoryExtensions
{
    public static void NoTracking<TKey, TEntity, TRepository>(
        this IRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.NoTracking;
    }

    public static void NoTrackingWithIdentityResolution<TKey, TEntity,
        TRepository>(
        this IRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior =
            TrackingBehavior.NoTrackingWithIdentityResolution;
    }

    public static void TrackAll<TKey, TEntity, TRepository>(
        this IRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.TrackAll;
    }

    public static void NoTracking<TKey, TEntity, TRepository>(
        this ISyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : ISyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.NoTracking;
    }

    public static void NoTrackingWithIdentityResolution<TKey, TEntity,
        TRepository>(
        this ISyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : ISyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior =
            TrackingBehavior.NoTrackingWithIdentityResolution;
    }

    public static void TrackAll<TKey, TEntity, TRepository>(
        this ISyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : ISyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.TrackAll;
    }

    public static void NoTracking<TKey, TEntity, TRepository>(
        this IAsyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IAsyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.NoTracking;
    }

    public static void NoTrackingWithIdentityResolution<TKey, TEntity,
        TRepository>(
        this IAsyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IAsyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior =
            TrackingBehavior.NoTrackingWithIdentityResolution;
    }

    public static void TrackAll<TKey, TEntity, TRepository>(
        this IAsyncRepository<TKey, TEntity, TRepository> repository
    )
        where TEntity : class, IEntity<TKey>
        where TRepository : IAsyncRepository<TKey, TEntity, TRepository>
    {
        repository.TrackingBehavior = TrackingBehavior.TrackAll;
    }
}