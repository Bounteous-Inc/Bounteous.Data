using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContextFactory<out  T> where T : IDbContext<Guid>
{
    T Create();
}

public abstract class DbContextFactory<T> : IDbContextFactory<T> where T : IDbContext<Guid>
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