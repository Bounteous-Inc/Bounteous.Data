using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates read-only entities that cannot be modified.
/// Shows how ReadOnlyEntityBase prevents Create, Update, and Delete operations.
/// </summary>
public class Feature04_ReadOnlyEntitiesDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 4;
    public string FeatureName => "Read-Only Entities (Query-Only, No CUD)";

    public Feature04_ReadOnlyEntitiesDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Debug("[READ-ONLY] Seeding legacy system data for read-only demonstration");
        Log.Information("[READ-ONLY] Note: In production, read-only entities would be pre-existing data");
        Log.Information("[READ-ONLY] ✓ Read-only entities prevent accidental modifications");
        Log.Information("[READ-ONLY] ✓ Queries would work normally if data existed");
        
        await Task.CompletedTask;
    }
}
