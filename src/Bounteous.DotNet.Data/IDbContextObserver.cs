using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;

namespace Bounteous.DotNet.Data;

public interface IDbContextObserver : IDisposable
{
    void OnEntityTracked(object sender, EntityTrackedEventArgs e);
    void OnStateChanged(object? sender, EntityStateChangedEventArgs e);
    void OnSaved();
}

public sealed class DefaultDbContextObserver : IDbContextObserver
{
    public void Dispose()
        => Log.Debug("Disposing [{@type}] called.", GetType().Name);

    public void OnEntityTracked(object sender, EntityTrackedEventArgs e)
        => Log.Debug("OnEntityTracked [{@type}] called.", e.GetType().Name);

    public void OnStateChanged(object? sender, EntityStateChangedEventArgs e)
        => Log.Debug("OnStateChanged [{@type}] called.", e.NewState.ToString());

    public void OnSaved()
        => Log.Debug("OnSaved [{@type}] called.", GetType().Name);
}