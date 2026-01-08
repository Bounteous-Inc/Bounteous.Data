using System.Linq.Expressions;
using Bounteous.Data.Domain;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;

#pragma warning disable CS8603 
    public static IQueryable<T> IncludeIf<T>(this IQueryable<T> source, bool condition,
        Expression<Func<T, object>> navigationPropertyPath) where T : class
     => condition ? source.Include(navigationPropertyPath) : source;
#pragma warning restore CS8603

    extension<T>(IQueryable<T> query)
    {
        public IAsyncEnumerable<T> ToPaginatedEnumerableAsync(int page = 1, 
            int size = 50)
            => query.Skip((page - 1) * size).Take(size).AsAsyncEnumerable();

        public async Task<List<T>> ToPaginatedListAsync(int page = 1, int size = 50)
            => await query.Skip((page - 1) * size).Take(size).ToListAsync();
    }

    public static async Task<T> FindById<T>(this DbSet<T> dbSet, Guid id, 
        params Expression<Func<T, object>>[] includes) where T : class, IEntity<Guid>
    {
        var query = includes.Aggregate<Expression<Func<T, object>>?, IQueryable<T>>(dbSet,
            (current, include) => current.Include(include!));

        var entity = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        return entity ?? throw new NotFoundException<T>(id);
    }

    public static async Task<T> FindById<T, TId>(this DbSet<T> dbSet, TId id, 
        params Expression<Func<T, object>>[] includes) where T : class, IEntity<TId>
    {
        var query = includes.Aggregate<Expression<Func<T, object>>?, IQueryable<T>>(dbSet,
            (current, include) => current.Include(include!));

        var entity = await query.FirstOrDefaultAsync(e => EF.Property<TId>(e, "Id")!.Equals(id));
        return entity ?? throw new NotFoundException<T, TId>(id);
    }
}