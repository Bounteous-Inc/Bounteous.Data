using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions;

/// <summary>
/// Extension methods for DbContext to provide fluent API for read-only scopes.
/// </summary>
public static class DbContextScopeExtensions
{
    /// <summary>
    /// Suppresses read-only entity validation for test data seeding.
    /// Use this in unit tests when you need to seed read-only entities into the database.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <returns>A disposable scope that suppresses validation. Dispose to re-enable validation.</returns>
    /// <remarks>
    /// This is intended for TEST USE ONLY. It allows you to seed read-only entities
    /// that would normally throw exceptions on SaveChanges.
    /// 
    /// Example:
    /// <code>
    /// using var scope = context.SuppressReadOnlyValidation();
    /// context.ReadOnlyEntities.Add(testData);
    /// await context.SaveChangesAsync(); // Succeeds
    /// </code>
    /// </remarks>
    public static ReadOnlyValidationScope SuppressReadOnlyValidation(this DbContext context)
    {
        return new ReadOnlyValidationScope();
    }

    /// <summary>
    /// Enforces read-only mode for query-only operations.
    /// Any attempt to save changes within this scope will throw an exception.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <returns>A disposable scope that enforces read-only mode. Dispose to allow normal operations.</returns>
    /// <remarks>
    /// Use this for query-only operations like GET endpoints, reports, or list views.
    /// It prevents accidental data modifications by throwing an exception if SaveChanges is called.
    /// 
    /// Example:
    /// <code>
    /// using var scope = context.EnforceReadOnly();
    /// var companies = await context.Companies.ToListAsync(); // OK
    /// await context.SaveChangesAsync(); // Throws InvalidOperationException
    /// </code>
    /// </remarks>
    public static ReadOnlyRequestScope EnforceReadOnly(this DbContext context)
    {
        return new ReadOnlyRequestScope();
    }

    /// <summary>
    /// Alias for SuppressReadOnlyValidation. Allows test data seeding of read-only entities.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <returns>A disposable scope that suppresses validation.</returns>
    public static ReadOnlyValidationScope AllowTestSeeding(this DbContext context)
    {
        return new ReadOnlyValidationScope();
    }

    /// <summary>
    /// Alias for EnforceReadOnly. Marks the context as query-only.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <returns>A disposable scope that enforces read-only mode.</returns>
    public static ReadOnlyRequestScope AsQueryOnly(this DbContext context)
    {
        return new ReadOnlyRequestScope();
    }
}
