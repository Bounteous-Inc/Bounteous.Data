using System.Linq.Expressions;
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

    // Async query extension methods that delegate to EF Core's implementations
    
    /// <summary>
    /// Asynchronously returns all elements of a sequence as a List.
    /// </summary>
    public static Task<List<TEntity>> ToListAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns an array of all elements in a sequence.
    /// </summary>
    public static Task<TEntity[]> ToArrayAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence.
    /// </summary>
    public static Task<TEntity> FirstAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.FirstAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a condition.
    /// </summary>
    public static Task<TEntity> FirstAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.FirstAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence, or a default value if no element is found.
    /// </summary>
    public static Task<TEntity?> FirstOrDefaultAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a condition, or a default value if no such element is found.
    /// </summary>
    public static Task<TEntity?> FirstOrDefaultAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence.
    /// </summary>
    public static Task<TEntity> SingleAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.SingleAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a condition.
    /// </summary>
    public static Task<TEntity> SingleAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.SingleAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence, or a default value if no element exists.
    /// </summary>
    public static Task<TEntity?> SingleOrDefaultAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a condition, or a default value if no such element exists.
    /// </summary>
    public static Task<TEntity?> SingleOrDefaultAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously determines whether a sequence contains any elements.
    /// </summary>
    public static Task<bool> AnyAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    public static Task<bool> AnyAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    public static Task<int> CountAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
    /// </summary>
    public static Task<int> CountAsync<TEntity, TId>(
        this ReadOnlyDbSet<TEntity, TId> source,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class, IReadOnlyEntity<TId>
    {
        return source.InnerDbSet.CountAsync(predicate, cancellationToken);
    }
}
