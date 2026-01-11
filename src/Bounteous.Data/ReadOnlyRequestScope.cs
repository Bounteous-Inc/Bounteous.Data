using System;

namespace Bounteous.Data;

/// <summary>
/// Enforces read-only mode for query-only request workflows.
/// Any attempt to save changes within this scope will throw an exception.
/// This is the inverse of ReadOnlyValidationScope - it enforces read-only behavior
/// rather than suppressing validation.
/// </summary>
/// <remarks>
/// Use this scope for request/response workflows that should only query data,
/// such as GET endpoints, reports, or list views. If any code attempts to modify
/// entities and call SaveChanges, an exception will be thrown.
/// 
/// Example usage:
/// <code>
/// public async Task&lt;List&lt;Company&gt;&gt; GetCompaniesAsync()
/// {
///     using (new ReadOnlyRequestScope())
///     using (var context = contextFactory.Create())
///     {
///         // Only queries allowed - SaveChanges will throw
///         return await context.Companies.ToListAsync();
///     }
/// }
/// </code>
/// </remarks>
public sealed class ReadOnlyRequestScope : IDisposable
{
    private static readonly AsyncLocal<bool> _isActive = new();
    
    /// <summary>
    /// Gets whether a read-only request scope is currently active.
    /// </summary>
    public static bool IsActive => _isActive.Value;
    
    /// <summary>
    /// Initializes a new read-only request scope.
    /// Any SaveChanges calls within this scope will throw an exception.
    /// </summary>
    public ReadOnlyRequestScope()
    {
        _isActive.Value = true;
    }
    
    /// <summary>
    /// Exits the read-only request scope, allowing normal save operations.
    /// </summary>
    public void Dispose()
    {
        _isActive.Value = false;
    }
}
