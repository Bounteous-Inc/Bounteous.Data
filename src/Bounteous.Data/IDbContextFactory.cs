using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContextFactory<out T, TUserId> where T : IDbContext<TUserId> where TUserId : struct
{
    T Create();
}

public abstract class DbContextFactory<T, TUserId> : IDbContextFactory<T, TUserId> 
    where T : IDbContext<TUserId> 
    where TUserId : struct
{
    protected readonly IConnectionBuilder ConnectionBuilder;
    protected readonly IDbContextObserver Observer;
    protected readonly IIdentityProvider<TUserId>? IdentityProvider;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected DbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
    {
        ConnectionBuilder = connectionBuilder;
        Observer = observer;
    }

    protected DbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer, IIdentityProvider<TUserId>? identityProvider)
        : this(connectionBuilder, observer)
    {
        IdentityProvider = identityProvider;
    }
    
    public T Create() => Create(ApplyOptions(), Observer, IdentityProvider);
    protected abstract T Create(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<TUserId>? identityProvider);
    protected abstract DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false);
}