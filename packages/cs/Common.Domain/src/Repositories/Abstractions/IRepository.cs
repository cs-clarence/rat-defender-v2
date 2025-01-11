using Common.Domain.Entities.Abstractions;

namespace Common.Domain.Repositories.Abstractions;

public interface IRepository<in TKey, TEntity, out TRepository>
    : IAsyncRepository<TKey, TEntity, TRepository>,
        ISyncRepository<TKey, TEntity, TRepository>
    where TEntity : class, IEntity<TKey>
    where TRepository : IRepository<TKey, TEntity, TRepository>
{
    public new TrackingBehavior TrackingBehavior { get; set; }
    public new bool IgnoreAutoIncludes { get; set; }

    public new TRepository ToNoTracking();
    public new TRepository ToNoTrackingWithIdentityResolution();
    public new TRepository ToTracking();

    public new TRepository ToIgnoreAutoIncludes(
        bool ignore = true
    );

    public new IQueryable<TEntity> Entities { get; }

    TrackingBehavior IAsyncRepository<TKey, TEntity, TRepository>.
        TrackingBehavior
    {
        get => TrackingBehavior;
        set => TrackingBehavior = value;
    }

    bool IAsyncRepository<TKey, TEntity, TRepository>.IgnoreAutoIncludes
    {
        get => IgnoreAutoIncludes;
        set => IgnoreAutoIncludes = value;
    }

    TRepository IAsyncRepository<
        TKey,
        TEntity,
        TRepository
    >.ToNoTracking()
    {
        return ToNoTracking();
    }

    TRepository IAsyncRepository<
        TKey,
        TEntity,
        TRepository
    >.ToNoTrackingWithIdentityResolution()
    {
        return ToNoTrackingWithIdentityResolution();
    }

    TRepository IAsyncRepository<TKey, TEntity, TRepository>.ToTracking()
    {
        return ToTracking();
    }

    TRepository IAsyncRepository<
        TKey,
        TEntity,
        TRepository
    >.ToIgnoreAutoIncludes(bool ignore)
    {
        return ToIgnoreAutoIncludes(ignore);
    }

    TrackingBehavior ISyncRepository<TKey, TEntity, TRepository>.
        TrackingBehavior
    {
        get => TrackingBehavior;
        set => TrackingBehavior = value;
    }

    bool ISyncRepository<TKey, TEntity, TRepository>.IgnoreAutoIncludes
    {
        get => IgnoreAutoIncludes;
        set => IgnoreAutoIncludes = value;
    }

    TRepository ISyncRepository<TKey, TEntity, TRepository>.ToNoTracking()
    {
        return ToNoTracking();
    }

    TRepository ISyncRepository<
        TKey,
        TEntity,
        TRepository
    >.ToNoTrackingWithIdentityResolution()
    {
        return ToNoTrackingWithIdentityResolution();
    }

    TRepository ISyncRepository<TKey, TEntity, TRepository>.ToTracking()
    {
        return ToTracking();
    }

    TRepository ISyncRepository<
        TKey,
        TEntity,
        TRepository
    >.ToIgnoreAutoIncludes(bool ignore)
    {
        return ToIgnoreAutoIncludes(ignore);
    }

    IQueryable<TEntity> ISyncRepository<TKey, TEntity, TRepository>.Entities =>
        Entities;

    IQueryable<TEntity> IAsyncRepository<TKey, TEntity, TRepository>.Entities =>
        Entities;
}