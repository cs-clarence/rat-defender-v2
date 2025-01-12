using System.Linq.Expressions;
using Common.Domain.Entities.Abstractions;
using Common.Domain.Repositories.Abstractions.Builders;

namespace Common.Domain.Repositories.Abstractions;

public interface ISyncRepository<in TKey, TEntity, out TRepository>
    where TEntity : class, IEntity<TKey>
    where TRepository : ISyncRepository<TKey, TEntity, TRepository>
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

    TEntity? FindById(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TEntity GetById(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TProjected? FindProjectedById<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TProjected GetProjectedById<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TEntity? Find(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TProjected? FindProjected<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TEntity Get(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    TProjected GetProjected<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    );

    ICollection<TEntity> GetAll(
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    );

    ICollection<TProjected> GetAllProjected<TProjected>(
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    );

    ICollection<TEntity> FindMany(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    );

    ICollection<TProjected> FindManyProjected<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    );

    void Add(TEntity entity);

    void AddMany(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void UpdateMany(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveMany(IEnumerable<TEntity> entities);
    void RemoveMany(Expression<Func<TEntity, bool>> predicate);

    void RemoveById(TKey id);

    bool Contains(Expression<Func<TEntity, bool>> predicate);

    bool ContainsById(TKey id);

    void LoadReference<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector
    )
        where TProperty : class;

    void LoadReference<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class;

    void LoadReference<TProperty>(TEntity entity, string propertyName)
        where TProperty : class;

    void LoadReference<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class;

    void LoadCollection<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector
    )
        where TProperty : class;

    void LoadCollection<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class;

    void LoadCollection<TProperty>(TEntity entity, string propertyName)
        where TProperty : class;

    void LoadCollection<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class;

    ulong Count(Expression<Func<TEntity, bool>> predicate);

    ulong CountAsync();
}