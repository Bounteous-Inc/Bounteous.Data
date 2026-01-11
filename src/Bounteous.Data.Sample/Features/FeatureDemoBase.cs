using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Base class for feature demonstrations that provides common functionality
/// and reduces code duplication while keeping each feature class intent-revealing.
/// </summary>
public abstract class FeatureDemoBase : IFeatureDemo
{
    protected readonly IServiceProvider ServiceProvider;

    public abstract int FeatureNumber { get; }
    public abstract string FeatureName { get; }

    protected FeatureDemoBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        LogFeatureHeader();
        await ExecuteFeatureAsync(userId);
    }

    /// <summary>
    /// Implement this method in derived classes to execute the feature demonstration.
    /// </summary>
    protected abstract Task ExecuteFeatureAsync(Guid userId);

    /// <summary>
    /// Logs the feature header with consistent formatting.
    /// </summary>
    protected void LogFeatureHeader()
    {
        Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
        Log.Information("║ FEATURE {Number}: {Name,-54} ║", FeatureNumber, FeatureName);
        Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    }

    /// <summary>
    /// Helper method to get a required service from the service provider.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Helper method to log a feature-specific message with a tag.
    /// </summary>
    protected void LogFeature(string tag, string message, params object[] args)
    {
        Log.Information($"[{tag}] {message}", args);
    }

    /// <summary>
    /// Helper method to log a feature-specific debug message with a tag.
    /// </summary>
    protected void LogDebug(string tag, string message, params object[] args)
    {
        Log.Debug($"[{tag}] {message}", args);
    }
}
