using System.Linq.Expressions;
using Bounteous.DotNet.Data.Domain;
using Bounteous.DotNet.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.DotNet.Data.Queries;

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

}