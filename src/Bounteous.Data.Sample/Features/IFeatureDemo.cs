namespace Bounteous.Data.Sample.Features;

/// <summary>
/// Interface for feature demonstration classes.
/// Each feature should implement this to provide a consistent execution pattern.
/// </summary>
public interface IFeatureDemo
{
    /// <summary>
    /// Gets the feature number (e.g., 1, 2, 3...).
    /// </summary>
    int FeatureNumber { get; }
    
    /// <summary>
    /// Gets the feature name/title.
    /// </summary>
    string FeatureName { get; }
    
    /// <summary>
    /// Executes the feature demonstration.
    /// </summary>
    /// <param name="userId">The user ID for audit tracking.</param>
    Task ExecuteAsync(Guid userId);
}
