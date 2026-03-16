using Bounteous.Data.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bounteous.Data.Extensions;

public static class DbSetExtensions
{
    public static void Delete<TEntity>(this DbSet<TEntity> dbSet, TEntity entity)
        where TEntity : class, ISoftDelete
    {
        // Mark entity as already deleted to signal physical deletion
        entity.IsDeleted = true;
        dbSet.Remove(entity);
    }

    public static void DeleteRange<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities)
        where TEntity : class, ISoftDelete
    {
        foreach (var entity in entities)
        {
            // Mark entity as already deleted to signal physical deletion
            entity.IsDeleted = true;
        }
        dbSet.RemoveRange(entities);
    }
}
