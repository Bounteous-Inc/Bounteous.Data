using System.Linq.Expressions;
using Bounteous.Data.Domain;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Queries;

public static class QueryExtensions
{
    public static async Task<T> FindById<T>(this DbSet<T> dbSet, Guid id, 
        params Expression<Func<T, object>>[] includes) where T : class, IAuditable
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