using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContextFactory<out T, TUserId> where T : IDbContext<TUserId> where TUserId : struct
{
    T Create();
}

public abstract class DbContextFactory<T, TUserId>(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<TUserId> identityProvider)
    : IDbContextFactory<T, TUserId>
    where T : IDbContext<TUserId>
    where TUserId : struct
{
    protected readonly IConnectionBuilder ConnectionBuilder = connectionBuilder;
    protected readonly IDbContextObserver Observer = observer;
    protected readonly IIdentityProvider<TUserId> IdentityProvider = identityProvider;

    public T Create() => Create(ApplyOptions(), Observer, IdentityProvider);
    protected abstract T Create(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<TUserId> identityProvider);
    protected abstract DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false);
}