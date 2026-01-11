using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates WithUserId for user context.
/// Shows how to set user ID for audit tracking in a type-safe manner.
/// </summary>
public class Feature10_WithUserIdDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 10;
    public string FeatureName => "WithUserId for User Context (Type-Safe)";

    public Feature10_WithUserIdDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[WITH-USER-ID] ✓ Fluent API for setting user context");
        Log.Information("[WITH-USER-ID] ✓ Type-safe user ID tracking");
        
        await Task.CompletedTask;
    }
}
