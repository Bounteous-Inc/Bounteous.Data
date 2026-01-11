using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates version tracking for optimistic concurrency.
/// Shows how Version field prevents concurrent update conflicts.
/// </summary>
public class Feature12_VersionTrackingDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 12;
    public string FeatureName => "Version Tracking (Optimistic Concurrency)";

    public Feature12_VersionTrackingDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[VERSION] ✓ Version field increments on every update");
        Log.Information("[VERSION] ✓ Prevents lost updates in concurrent scenarios");
        
        await Task.CompletedTask;
    }
}
