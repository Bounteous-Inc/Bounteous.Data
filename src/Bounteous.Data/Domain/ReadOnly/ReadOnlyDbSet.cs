using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Domain.ReadOnly;

/// <summary>
/// A read-only wrapper around DbSet that provides fail-fast write protection.
/// Throws ReadOnlyEntityException immediately when Add, Remove, Update, or Attach operations are attempted.
/// Supports LINQ queries and async operations through extension methods.
/// 
/// USAGE:
/// <code>
/// public ReadOnlyDbSet&lt;LegacySystem, int&gt; LegacySystems 
///     =&gt; Set&lt;LegacySystem&gt;().AsReadOnly&lt;LegacySystem, int&gt;();
/// 
/// // Async operations work directly via extension methods:
/// var systems = await context.LegacySystems.ToListAsync();
/// var system = await context.LegacySystems.FirstOrDefaultAsync(s => s.Id == 1);
/// </code>
/// </summary>
public class ReadOnlyDbSet<TEntity, TId> : IQueryable<TEntity>
    where TEntity : class, IReadOnlyEntity<TId>
{
    private readonly DbSet<TEntity> innerDbSet;
    private readonly string entityTypeName;

    internal DbSet<TEntity> InnerDbSet => innerDbSet;

    public ReadOnlyDbSet(DbSet<TEntity> dbSet)
    {
        innerDbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        entityTypeName = typeof(TEntity).Name;
    }

    // IQueryable<TEntity> - forwards to inner DbSet for LINQ support
    public Type ElementType => ((IQueryable<TEntity>)innerDbSet).ElementType;
    public Expression Expression => ((IQueryable<TEntity>)innerDbSet).Expression;
    public IQueryProvider Provider => ((IQueryable<TEntity>)innerDbSet).Provider;

    // IEnumerable<TEntity> - forwards to inner DbSet
    public IEnumerator<TEntity> GetEnumerator() => innerDbSet.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)innerDbSet.AsEnumerable()).GetEnumerator();

    public static explicit operator DbSet<TEntity>(ReadOnlyDbSet<TEntity, TId> readOnlySet)
        => readOnlySet.innerDbSet;

    // Mutating operations - all throw immediately
    public EntityEntry<TEntity> Add(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public void AddRange(params TEntity[] entities)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public void AddRange(IEnumerable<TEntity> entities)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public Task AddRangeAsync(params TEntity[] entities)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => throw new ReadOnlyEntityException(entityTypeName, "create");

    public EntityEntry<TEntity> Attach(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "attach");

    public void AttachRange(params TEntity[] entities)
        => throw new ReadOnlyEntityException(entityTypeName, "attach");

    public void AttachRange(IEnumerable<TEntity> entities)
        => throw new ReadOnlyEntityException(entityTypeName, "attach");

    public EntityEntry<TEntity> Remove(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "delete");

    public void RemoveRange(params TEntity[] entities)
        => throw new ReadOnlyEntityException(entityTypeName, "delete");

    public void RemoveRange(IEnumerable<TEntity> entities)
        => throw new ReadOnlyEntityException(entityTypeName, "delete");

    public EntityEntry<TEntity> Update(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "update");

    public void UpdateRange(params TEntity[] entities)
        => throw new ReadOnlyEntityException(entityTypeName, "update");

    public void UpdateRange(IEnumerable<TEntity> entities)
        => throw new ReadOnlyEntityException(entityTypeName, "update");
}
