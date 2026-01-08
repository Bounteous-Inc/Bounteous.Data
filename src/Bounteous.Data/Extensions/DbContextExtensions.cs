using Bounteous.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions;

public static class DbContextExtensions
{
    public static TDomain CreateNew<TModel, TDomain>(this DbContext context, TModel model) 
        where TModel : class
        where TDomain : class, IAuditable, new()
    {
        var entity = new TDomain();
        context.Entry(entity).CurrentValues.SetValues(model);
        return entity;
    }

    public static TDomain CreateNew<TModel, TDomain, TId>(this DbContext context, TModel model) 
        where TModel : class
        where TDomain : class, IAuditable<TId, Guid>, new()
    {
        var entity = new TDomain();
        context.Entry(entity).CurrentValues.SetValues(model);
        return entity;
    }

    public static TDomain AddNew<TDomain>(this DbContext dbContext, TDomain newEntity)
        where TDomain : class, IAuditable, new()
    {
        var entity = dbContext.Set<TDomain>().Add(newEntity);
        return entity.Entity;
    }

    public static TDomain AddNew<TDomain, TId>(this DbContext dbContext, TDomain newEntity)
        where TDomain : class, IAuditable<TId, Guid>, new()
    {
        var entity = dbContext.Set<TDomain>().Add(newEntity);
        return entity.Entity;
    }

    public static DbSet<TDomain> DbSet<TDomain>(this DbContext dbContext) 
        where TDomain : class, IAuditable, new()
        => dbContext.Set<TDomain>();

    public static DbSet<TDomain> DbSet<TDomain, TId>(this DbContext dbContext) 
        where TDomain : class, IAuditable<TId, Guid>, new()
        => dbContext.Set<TDomain>();
}