using System.Linq.Expressions;
using Common.Domain.Entities.Abstractions;
using Common.Domain.Exceptions;
using Common.Domain.Repositories.Abstractions;
using Common.Domain.Repositories.Abstractions.Builders;
using Common.Infrastructure.Persistence.Builders;
using Common.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Repositories;

public abstract class
    EfRepository<TKey, TEntity, TRepository> : IRepository<TKey, TEntity,
    TRepository>
    where TEntity : class, IEntity<TKey>
    where TRepository : IRepository<TKey, TEntity, TRepository>
{
    private QueryTrackingBehavior? _queryTrackingBehavior;

    protected DbContext DbContext => _dbContext;
    protected DbSet<TEntity> DbSet { get; }
    private readonly DbContext _dbContext;

    protected EfRepository(
        DbContext dbContext,
        ActiveDbContextCollection dbContexts
    )
    {
        _dbContext = dbContext;
        _queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;
        DbSet = dbContext.Set<TEntity>();

        if (!dbContexts.Contains(dbContext))
        {
            dbContexts.Add(dbContext);
        }
    }

    public bool IgnoreAutoIncludes { get; set; }

    protected IQueryable<TEntity> Queryable
    {
        get
        {
            var queryable = DbSet.AsSplitQuery();

            if (_queryTrackingBehavior is not null)
            {
                queryable = _queryTrackingBehavior switch
                {
                    QueryTrackingBehavior.TrackAll => DbSet.AsTracking(),
                    QueryTrackingBehavior.NoTracking => DbSet.AsNoTracking(),
                    QueryTrackingBehavior.NoTrackingWithIdentityResolution =>
                        DbSet.AsNoTrackingWithIdentityResolution(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }

            if (IgnoreAutoIncludes)
            {
                queryable = queryable.IgnoreAutoIncludes();
            }

            return queryable;
        }
    }

    public TRepository ToIgnoreAutoIncludes(bool ignore = true)
    {
        var cloned = Clone();

        cloned.IgnoreAutoIncludes = ignore;
        return cloned;
    }

    public IQueryable<TEntity> Entities => Queryable;

    public TrackingBehavior TrackingBehavior
    {
        get
        {
            return _queryTrackingBehavior switch
            {
                QueryTrackingBehavior.TrackAll => TrackingBehavior.TrackAll,
                QueryTrackingBehavior.NoTracking => TrackingBehavior.NoTracking,
                QueryTrackingBehavior.NoTrackingWithIdentityResolution =>
                    TrackingBehavior.NoTrackingWithIdentityResolution,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        set
        {
            switch (value)
            {
                case TrackingBehavior.TrackAll:
                    _queryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                    break;
                case TrackingBehavior.NoTracking:
                    _queryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    break;
                case TrackingBehavior.NoTrackingWithIdentityResolution:
                    _queryTrackingBehavior =
                        QueryTrackingBehavior.NoTrackingWithIdentityResolution;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        null
                    );
            }
        }
    }

    private TRepository Clone()
    {
        return (TRepository)MemberwiseClone();
    }

    public TRepository ToTracking()
    {
        var cloned = Clone();

        cloned.TrackingBehavior = TrackingBehavior;
        return cloned;
    }

    public TRepository ToNoTracking()
    {
        var cloned = Clone();

        cloned.TrackingBehavior = TrackingBehavior.NoTracking;
        return cloned;
    }

    public TRepository ToNoTrackingWithIdentityResolution()
    {
        var cloned = Clone();

        cloned.TrackingBehavior =
            TrackingBehavior.NoTrackingWithIdentityResolution;
        return cloned;
    }

    public Task<TProjected?> FindProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);

        return query
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.FindAsync(
            [id],
            cancellationToken: cancellationToken
        );
    }

    public Task<TEntity?> FindByIdAsync(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);

        return query
            .Where(i => i.Id!.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjected> GetProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);

        var projected = await query
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);

        if (projected is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return projected;
    }

    public async Task<TEntity> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        var entity = await FindByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            var type = typeof(TEntity);
            var domain = type.Namespace?.Split('.').First() ?? "Unknown";
            throw EntityNotFoundException.ForId(
                type.Name,
                $"{id}",
                domain
            );
        }

        return entity;
    }

    public async Task<TEntity> GetByIdAsync(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await FindByIdAsync(id, include, cancellationToken);
        if (entity is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} with '{id}' was not found",
                "Unknown"
            );
        }

        return entity;
    }

    public Task<TProjected?> FindProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        return Queryable
            .Where(i => i.Id!.Equals(id))
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TProjected?> FindProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);
        return query
            .Where(i => i.Id!.Equals(id))
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await Queryable
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return entity;
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);
        var entity = await query
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return entity;
    }

    public async Task<TProjected> GetProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        var projected = await Queryable
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);

        if (projected is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return projected;
    }

    public async Task<TProjected> GetProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        var selected = await Queryable
            .Where(i => i.Id!.Equals(id))
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);

        if (selected is null)
        {
            throw EntityNotFoundException.ForId(
                typeof(TEntity).Name,
                $"{id}",
                "Unknown"
            );
        }

        return selected;
    }

    public async Task<TProjected> GetProjectedByIdAsync<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include);
        var projected = await query
            .Where(i => i.Id!.Equals(id))
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);

        if (projected is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} with '{id}' was not found",
                "Unknown"
            );
        }

        return projected;
    }

    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return Queryable
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        > include,
        CancellationToken cancellationToken = default
    )
    {
        return ApplyBuilders(Queryable, null, include)
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TProjected?> FindProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        return Queryable
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await Queryable.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TProjected>> GetAllProjectedAsync<TProjected>(
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        return await Queryable.Select(select).ToListAsync(cancellationToken);
    }

    private static IQueryable<TEntity> ApplyBuilders(
        IQueryable<TEntity> queryable,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    )
    {
        var returnQueryable = queryable;

        if (inclusion != null)
        {
            var inclusionBuilder = inclusion(
                new QueryInclusionBuilder<TEntity>(returnQueryable)
            );
            if (inclusionBuilder is QueryInclusionBuilder<TEntity> concrete)
            {
                returnQueryable = concrete.GetQueryable();
            }
            else
            {
                throw new InvalidOperationException(
                    "inclusion lambda must return the passed builder"
                );
            }
        }

        if (ordering != null)
        {
            var orderingBuilder = ordering(
                new QueryOrderingBuilder<TEntity>(returnQueryable)
            );
            if (orderingBuilder is QueryOrderingBuilder<TEntity> concrete)
            {
                returnQueryable = concrete.GetQueryable();
            }
            else
            {
                throw new InvalidOperationException(
                    "ordering lambda must return the passed builder"
                );
            }
        }

        return returnQueryable;
    }

    public async Task<ICollection<TEntity>> GetAllAsync(
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? order = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = ApplyBuilders(Queryable, order, include);

        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TProjected>> GetAllProjectedAsync<TProjected>(
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
    )
    {
        var queryable = ApplyBuilders(Queryable, order, include);
        return await queryable.Select(select).ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> FindManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await Queryable.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<
        ICollection<TProjected>
    > FindManyProjectedAsync<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = Queryable.Where(predicate);
        return await queryable.Select(select).ToListAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> FindManyAsync(
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
    )
    {
        var queryable = ApplyBuilders(
            Queryable.Where(predicate),
            order,
            include
        );

        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<
        ICollection<TProjected>
    > FindManyProjectedAsync<TProjected>(
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
    )
    {
        var queryable = ApplyBuilders(
            Queryable.Where(predicate),
            order,
            include
        );
        return await queryable.Select(select).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        var entitiesToAdd = entities.ToList();
        await DbSet.AddRangeAsync(entitiesToAdd, cancellationToken);
    }

    public Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        DbSet.Update(entity);

        return Task.CompletedTask;
    }

    public Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        var entitiesToUpdate = entities.ToList();
        DbSet.UpdateRange(entitiesToUpdate);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        DbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public Task RemoveManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        DbSet.RemoveRange(entities);

        return Task.CompletedTask;
    }

    public Task RemoveManyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return DbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RemoveByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await DbSet.FindAsync([id], cancellationToken);

        if (entity == null)
            return;

        DbSet.Remove(entity);
    }

    public async Task<bool> ContainsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await Queryable.AnyAsync(predicate, cancellationToken);
    }

    public Task<bool> ContainsByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default
    )
    {
        return Queryable.AnyAsync(i => i.Id!.Equals(id), cancellationToken);
    }

    private void AttachIfDetached(TEntity entity)
    {
        if (DbContext.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }
    }

    public async Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await DbContext
            .Entry(entity)
            .Reference(propertySelector)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await additional(
                DbContext.Entry(entity).Reference(propertySelector).Query()
            )
            .LoadAsync(cancellationToken);
    }

    public async Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        string propertyName,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await DbContext
            .Entry(entity)
            .Reference<TProperty>(propertyName)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadReferenceAsync<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await additional(
                DbContext
                    .Entry(entity)
                    .Reference<TProperty>(propertyName)
                    .Query()
            )
            .LoadAsync(cancellationToken);
    }

    public async Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await DbContext
            .Entry(entity)
            .Collection(propertySelector)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await additional(
                DbContext.Entry(entity).Collection(propertySelector).Query()
            )
            .LoadAsync(cancellationToken);
    }

    public async Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        string propertyName,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await DbContext
            .Entry(entity)
            .Collection<TProperty>(propertyName)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadCollectionAsync<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional,
        CancellationToken cancellationToken = default
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        await additional(
                DbContext
                    .Entry(entity)
                    .Collection<TProperty>(propertyName)
                    .Query()
            )
            .LoadAsync(cancellationToken);
    }

    public async Task<ulong> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return (ulong)await Queryable.CountAsync(predicate, cancellationToken);
    }

    public async Task<ulong> CountAsync(
        CancellationToken cancellationToken = default
    )
    {
        return (ulong)await Queryable.CountAsync(cancellationToken);
    }

    public TEntity? FindById(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        return DbSet.Find(id);
    }

    public TEntity GetById(
        TKey id,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        ArgumentNullException.ThrowIfNull(id);
        var entity = FindById(id, include);

        if (entity is null)
        {
            throw EntityNotFoundException.ForId(
                typeof(TEntity).Name,
                $"{id}",
                "Unknown"
            );
        }

        return entity;
    }

    public TProjected? FindProjectedById<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include)
            .Where(i => i.Id!.Equals(id));
        return query.Select(select).FirstOrDefault();
    }

    public TProjected GetProjectedById<TProjected>(
        TKey id,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include)
            .Where(i => i.Id!.Equals(id))
            .Select(select)
            .FirstOrDefault();

        if (query is null)
        {
            throw EntityNotFoundException.ForId(
                typeof(TEntity).Name,
                $"{id}",
                "Unknown"
            );
        }

        return query;
    }

    public TEntity? Find(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        return ApplyBuilders(Queryable, null, include)
            .Where(predicate)
            .FirstOrDefault();
    }

    public TProjected? FindProjected<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include).Where(predicate);
        return query.Select(select).FirstOrDefault();
    }

    public TEntity Get(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        var entity = ApplyBuilders(Queryable, null, include)
            .Where(predicate)
            .FirstOrDefault();

        if (entity is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return entity;
    }

    public TProjected GetProjected<TProjected>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? include = default
    )
    {
        var query = ApplyBuilders(Queryable, null, include).Where(predicate);
        var projected = query.Select(select).FirstOrDefault();

        if (projected is null)
        {
            var entityName = typeof(TEntity).Name;
            throw new EntityNotFoundException(
                entityName,
                $"{entityName} for a condition was not found",
                "Unknown"
            );
        }

        return projected;
    }

    public ICollection<TEntity> GetAll(
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    )
    {
        var queryable = ApplyBuilders(Queryable, ordering, inclusion);

        return queryable.ToList();
    }

    public ICollection<TProjected> GetAllProjected<TProjected>(
        Expression<Func<TEntity, TProjected>> select,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    )
    {
        var queryable = ApplyBuilders(Queryable, ordering, inclusion);
        return queryable.Select(select).ToList();
    }

    public ICollection<TEntity> FindMany(
        Expression<Func<TEntity, bool>> predicate,
        Func<
            IQueryOrderingBuilder<TEntity>,
            IQueryOrderingBuilder<TEntity>
        >? ordering = null,
        Func<
            IQueryInclusionBuilder<TEntity>,
            IQueryInclusionBuilder<TEntity>
        >? inclusion = null
    )
    {
        var queryable = ApplyBuilders(
            Queryable.Where(predicate),
            ordering,
            inclusion
        );

        return queryable.ToList();
    }

    public ICollection<TProjected> FindManyProjected<TProjected>(
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
    )
    {
        var queryable = ApplyBuilders(
            Queryable.Where(predicate),
            ordering,
            inclusion
        );
        return queryable.Select(select).ToList();
    }

    public void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public void AddMany(IEnumerable<TEntity> entities)
    {
        DbSet.AddRange(entities);
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void UpdateMany(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public void RemoveMany(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public void RemoveMany(Expression<Func<TEntity, bool>> predicate)
    {
        DbSet.Where(predicate).ExecuteDelete();
    }

    public void RemoveById(TKey id)
    {
        var entity = DbSet.Find(id);

        if (entity == null)
            return;

        DbSet.Remove(entity);
    }

    public bool Contains(Expression<Func<TEntity, bool>> predicate)
    {
        return Queryable.Any(predicate);
    }

    public bool ContainsById(TKey id)
    {
        return Queryable.Any(i => i.Id!.Equals(id));
    }

    public void LoadReference<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        DbContext.Entry(entity).Reference(propertySelector).Load();
    }

    public void LoadReference<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        additional(DbContext.Entry(entity).Reference(propertySelector).Query())
            .Load();
    }

    public void LoadReference<TProperty>(TEntity entity, string propertyName)
        where TProperty : class
    {
        AttachIfDetached(entity);

        DbContext.Entry(entity).Reference<TProperty>(propertyName).Load();
    }

    public void LoadReference<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        additional(
                DbContext
                    .Entry(entity)
                    .Reference<TProperty>(propertyName)
                    .Query()
            )
            .Load();
    }

    public void LoadCollection<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        DbContext.Entry(entity).Collection(propertySelector).Load();
    }

    public void LoadCollection<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertySelector,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        additional(DbContext.Entry(entity).Collection(propertySelector).Query())
            .Load();
    }

    public void LoadCollection<TProperty>(TEntity entity, string propertyName)
        where TProperty : class
    {
        AttachIfDetached(entity);

        DbContext.Entry(entity).Collection<TProperty>(propertyName).Load();
    }

    public void LoadCollection<TProperty>(
        TEntity entity,
        string propertyName,
        Func<IQueryable<TProperty>, IQueryable<TProperty>> additional
    )
        where TProperty : class
    {
        AttachIfDetached(entity);

        additional(
                DbContext
                    .Entry(entity)
                    .Collection<TProperty>(propertyName)
                    .Query()
            )
            .Load();
    }

    public ulong Count(Expression<Func<TEntity, bool>> predicate)
    {
        return (ulong)Queryable.Count(predicate);
    }

    public ulong CountAsync()
    {
        return (ulong)Queryable.Count();
    }
}