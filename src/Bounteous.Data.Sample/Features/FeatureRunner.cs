using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Orchestrates the execution of all feature demonstrations.
/// Discovers and runs all IFeatureDemo implementations in order.
/// </summary>
public class FeatureRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IFeatureDemo> _features;

    public FeatureRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _features = DiscoverFeatures();
    }

    /// <summary>
    /// Discovers all feature demo classes that implement IFeatureDemo.
    /// </summary>
    private List<IFeatureDemo> DiscoverFeatures()
    {
        var featureType = typeof(IFeatureDemo);
        var features = featureType.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && featureType.IsAssignableFrom(t))
            .Select(t => (IFeatureDemo)Activator.CreateInstance(t, _serviceProvider)!)
            .OrderBy(f => f.FeatureNumber)
            .ToList();

        return features;
    }

    /// <summary>
    /// Executes all discovered features in order.
    /// </summary>
    public async Task RunAllFeaturesAsync(Guid userId)
    {
        Log.Information("╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║   Bounteous.Data Comprehensive Feature Demonstration         ║");
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        Log.Information("\n[SETUP] Test User ID: {UserId}", userId);
        Log.Information("[SETUP] Discovered {Count} feature demonstrations\n", _features.Count);

        foreach (var feature in _features)
        {
            try
            {
                await feature.ExecuteAsync(userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ERROR] Feature {Number} ({Name}) failed", feature.FeatureNumber, feature.FeatureName);
                throw;
            }
        }

        PrintSummary();
    }

    /// <summary>
    /// Prints a summary of all validated features.
    /// </summary>
    private void PrintSummary()
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║                    FEATURE SUMMARY                            ║");
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");

        foreach (var feature in _features)
        {
            Log.Information("✓ FEATURE {Number,-2}: {Name}", feature.FeatureNumber, feature.FeatureName);
        }

        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║          ALL BOUNTEOUS.DATA FEATURES VALIDATED ✓              ║");
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    }
}
