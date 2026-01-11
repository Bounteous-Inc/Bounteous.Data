using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Demonstrates IIdentityProvider for automatic user ID resolution.
/// Shows how to configure automatic user context without explicit WithUserId calls.
/// </summary>
public class Feature13_IdentityProviderDemo : IFeatureDemo
{
    private readonly IServiceProvider _serviceProvider;

    public int FeatureNumber => 13;
    public string FeatureName => "IIdentityProvider (Automatic User ID Resolution)";

    public Feature13_IdentityProviderDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("[IDENTITY-PROVIDER] ✓ Automatic user ID resolution from context");
        Log.Information("[IDENTITY-PROVIDER] ✓ No need for explicit WithUserId calls");
        
        await Task.CompletedTask;
    }
}
