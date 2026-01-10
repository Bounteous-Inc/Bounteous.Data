using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Domain.ReadOnly;

/// <summary>
/// A lightweight read-only wrapper around DbSet that throws ReadOnlyEntityException immediately
/// when Add, Remove, Update, or Attach operations are attempted.
/// Implicitly converts to DbSet for all query operations, ensuring full EF Core compatibility.
/// </summary>
public class ReadOnlyDbSet<TEntity, TId> where TEntity : class, IReadOnlyEntity<TId>
{
    private readonly DbSet<TEntity> innerDbSet;
    private readonly string entityTypeName;

    public ReadOnlyDbSet(DbSet<TEntity> dbSet)
    {
        innerDbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        entityTypeName = typeof(TEntity).Name;
    }

    /// <summary>
    /// Implicit conversion to DbSet for query operations.
    /// This allows full LINQ support including async operations.
    /// </summary>
    public static implicit operator DbSet<TEntity>(ReadOnlyDbSet<TEntity, TId> readOnlySet)
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
