using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Domain.ReadOnly;

/// <summary>
/// A read-only wrapper around DbSet that provides fail-fast write protection.
/// Throws ReadOnlyEntityException immediately when Add, Remove, Update, or Attach operations are attempted.
/// Supports LINQ queries and safe async operations directly on the class.
/// 
/// USAGE:
/// <code>
/// public ReadOnlyDbSet&lt;LegacySystem, int&gt; LegacySystems 
///     =&gt; Set&lt;LegacySystem&gt;().AsReadOnly&lt;LegacySystem, int&gt;();
/// 
/// // Safe async operations work directly on the ReadOnlyDbSet:
/// var count = await context.LegacySystems.CountAsync();
/// var system = await context.LegacySystems.FirstOrDefaultAsync(s => s.Id == 1);
/// var hasAny = await context.LegacySystems.AnyAsync(s => s.IsActive);
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

    // Safe async query methods - single value returns only
    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a condition, or a default value if no such element is found.
    /// </summary>
    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => innerDbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    /// <summary>
    /// Asynchronously returns the first element of a sequence, or a default value if no element is found.
    /// </summary>
    public Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => innerDbSet.FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => innerDbSet.AnyAsync(predicate, cancellationToken);

    /// <summary>
    /// Asynchronously determines whether a sequence contains any elements.
    /// </summary>
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => innerDbSet.AnyAsync(cancellationToken);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
    /// </summary>
    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => innerDbSet.CountAsync(predicate, cancellationToken);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => innerDbSet.CountAsync(cancellationToken);

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
