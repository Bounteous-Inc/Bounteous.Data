namespace Bounteous.Data.Extensions;

public static class DbContextBaseExtensions
{
    public static T WithUserIdTyped<T, TUserId>(this T context, TUserId userId) 
        where T : DbContextBase<TUserId>
        where TUserId : struct
    {
        ((IDbContext<TUserId>)context).WithUserId(userId);
        return context;
    }
}
