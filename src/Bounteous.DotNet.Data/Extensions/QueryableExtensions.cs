using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.DotNet.Data.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;

    public static IQueryable<T> IncludeIf<T>(this IQueryable<T> source, bool condition,
        Expression<Func<T, object>> navigationPropertyPath) where T : class
        => condition ? source.Include(navigationPropertyPath) : source;

    public static  IAsyncEnumerable<T> ToPaginatedEnumerableAsync<T>(this IQueryable<T> query, int page = 1, 
                                                                     int size = 50)
        => query.Skip((page - 1) * size).Take(size).AsAsyncEnumerable();

    public static async Task<List<T>> ToPaginatedListAsync<T>(this IQueryable<T> query, int page = 1, int size = 50)
        => await query.Skip((page - 1) * size).Take(size).ToListAsync();
}