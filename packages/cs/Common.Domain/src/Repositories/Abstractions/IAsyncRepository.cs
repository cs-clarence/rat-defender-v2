using System.Linq.Expressions;
using Common.Domain.Entities.Abstractions;
using Common.Domain.Repositories.Abstractions.Builders;

namespace Common.Domain.Repositories.Abstractions;

public interface IAsyncRepository<in TKey, TEntity, out TRepository>
    where TEntity : class, IEntity<TKey>
    where TRepository : IAsyncRepository<TKey, TEntity, TRepository>
{
    public TrackingBehavior TrackingBehavior { get; set; }
    public bool IgnoreAutoIncludes { get; set; }

    public TRepository ToNoTracking();
    public TRepository ToNoTrackingWithIdentityResolution();
    public TRepository ToTracking();

    public TRepository ToIgnoreAutoIncludes(
        bool ignore = true
    );

    public IQueryable<TEntity> Entities { get; }

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TProjected?> FindProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<TProjected?> FindProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FindByIdAsync(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TProjected?> FindProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<TProjected?> FindProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TProjected> GetProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<TProjected> GetProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> GetByIdAsync(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<TProjected> GetProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<TProjected> GetProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TProjected>> GetAllProjectedAsync<TProjected>(
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> GetAllAsync(
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? order = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = null,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TProjected>> GetAllProjectedAsync<TProjected>(
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? order = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = null,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> FindManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TProjected>> FindManyProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> FindManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? order = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = null,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TProjected>> FindManyProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? order = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = null,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    Task AddManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    Task RemoveManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );
    
    Task RemoveManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task RemoveByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ContainsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<bool> ContainsByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );

    Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        string propertyName,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        string propertyName,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class;

    Task<ulong> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<ulong> CountAsync(CancellationToken cancellationToken = default);
}