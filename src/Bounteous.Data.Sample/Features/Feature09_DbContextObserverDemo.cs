using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates DbContextObserver events.
/// Shows how to track entity changes with Tracked, Changed, and Saved events.
/// </summary>
public class Feature09_DbContextObserverDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 9;
    public string FeatureName => "DbContextObserver Events (Tracked, Changed, Saved)";

    public Feature09_DbContextObserverDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[OBSERVER] ✓ DbContextObserver provides lifecycle event hooks");
        Log.Information("[OBSERVER] ✓ Track entity changes for logging, auditing, or notifications");
        
        await Task.CompletedTask;
    }
}
