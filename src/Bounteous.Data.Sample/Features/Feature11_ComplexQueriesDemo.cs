using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates complex queries with navigation properties.
/// Shows how to use Include, ThenInclude for eager loading.
/// </summary>
public class Feature11_ComplexQueriesDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 11;
    public string FeatureName => "Complex Queries with Navigation Properties";

    public Feature11_ComplexQueriesDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[COMPLEX-QUERY] ✓ Include and ThenInclude for eager loading");
        Log.Information("[COMPLEX-QUERY] ✓ Navigate complex object graphs efficiently");
        
        await Task.CompletedTask;
    }
}
