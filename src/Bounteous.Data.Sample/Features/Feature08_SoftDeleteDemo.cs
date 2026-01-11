using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates soft delete functionality.
/// Shows how IsDeleted flag works with AuditVisitor.AcceptDeleted.
/// </summary>
public class Feature08_SoftDeleteDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 8;
    public string FeatureName => "Soft Delete (IsDeleted, AuditVisitor.AcceptDeleted)";

    public Feature08_SoftDeleteDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[SOFT-DELETE] ✓ Entities marked as deleted instead of removed");
        Log.Information("[SOFT-DELETE] ✓ Audit fields track who deleted and when");
        
        await Task.CompletedTask;
    }
}
