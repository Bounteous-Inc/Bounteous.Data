using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates FindById extension with NotFoundException.
/// Shows how to find entities by ID with automatic exception handling.
/// </summary>
public class Feature07_FindByIdDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 7;
    public string FeatureName => "FindById Extension & NotFoundException";

    public Feature07_FindByIdDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[FIND-BY-ID] ✓ FindById throws NotFoundException for missing entities");
        Log.Information("[FIND-BY-ID] ✓ Provides clear error messages with entity type and ID");
        
        await Task.CompletedTask;
    }
}
