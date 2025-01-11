using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.UnitOfWork;

public class ActiveDbContextCollection : IEnumerable<DbContext>
{
    private readonly HashSet<DbContext> _activeDbContexts =
        new(ReferenceEqualityComparer.Instance);

    public void Add(DbContext dbContext)
    {
        _activeDbContexts.Add(dbContext);
    }

    public bool Contains(DbContext dbContext)
    {
        return _activeDbContexts.Contains(dbContext);
    }

    public IEnumerable<DbContext> GetAll()
    {
        return _activeDbContexts;
    }

    public IEnumerator<DbContext> GetEnumerator()
    {
        return _activeDbContexts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
