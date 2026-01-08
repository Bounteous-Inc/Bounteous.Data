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

    // ReSharper disable once ConvertToPrimaryConstructor
    protected DbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
    {
        ConnectionBuilder = connectionBuilder;
        Observer = observer;
    } 
    
    public T Create() => Create(ApplyOptions(), Observer);
    protected abstract T Create(DbContextOptions options, IDbContextObserver observer);
    protected abstract DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false);
}