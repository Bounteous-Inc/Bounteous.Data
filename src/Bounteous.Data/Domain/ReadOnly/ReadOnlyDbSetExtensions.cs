using Bounteous.Data.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Domain.ReadOnly;

public static class ReadOnlyDbSetExtensions
{
    /// <summary>
    /// Wraps a DbSet in a ReadOnlyDbSet to provide fail-fast validation on write operations.
    /// </summary>
    public static ReadOnlyDbSet<TEntity, TId> AsReadOnly<TEntity, TId>(this DbSet<TEntity> dbSet)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return new ReadOnlyDbSet<TEntity, TId>(dbSet);
    }
}
