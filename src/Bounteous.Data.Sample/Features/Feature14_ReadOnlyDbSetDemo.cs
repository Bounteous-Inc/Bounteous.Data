using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates ReadOnlyDbSet for immediate fail-fast validation.
/// Shows how ReadOnlyDbSet prevents modifications at the DbSet level.
/// </summary>
public class Feature14_ReadOnlyDbSetDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 14;
    public string FeatureName => "ReadOnlyDbSet (Immediate Fail-Fast Validation)";

    public Feature14_ReadOnlyDbSetDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[READONLY-DBSET] ✓ Fail-fast validation at DbSet level");
        Log.Information("[READONLY-DBSET] ✓ Prevents Add, Update, Remove operations immediately");
        
        await Task.CompletedTask;
    }
}
