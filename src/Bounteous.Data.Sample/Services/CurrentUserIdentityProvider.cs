using Bounteous.Data;

namespace Bounteous.Data.Sample.Services;

/// <summary>
/// Sample implementation of IIdentityProvider for demonstration purposes.
/// In a real application, you would inject IHttpContextAccessor or your authentication service.
/// 
/// This implementation uses a static user ID that can be set for testing purposes.
/// </summary>
public class SampleIdentityProvider : IIdentityProvider<Guid>
{
    private static Guid _currentUserId;

    /// <summary>
    /// Sets the current user ID for the sample application.
    /// In production, this would come from HttpContext.User claims or similar.
    /// </summary>
    public static void SetCurrentUserId(Guid userId)
    {
        _currentUserId = userId;
    }

    /// <summary>
    /// Clears the current user ID.
    /// </summary>
    public static void ClearCurrentUserId()
    {
        _currentUserId = default;
    }

    public Guid GetCurrentUserId()
    {
        return _currentUserId;
    }
}

/// <summary>
/// Example implementation showing how to integrate with ASP.NET Core.
/// This is commented out as a reference for real applications.
/// </summary>
/*
public class HttpContextIdentityProvider : IIdentityProvider<Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextIdentityProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim?.Value is string userId && Guid.TryParse(userId, out var id))
            return id;
        
        return null;
    }
}
*/

/// <summary>
/// Example for long-based user IDs (e.g., SQL Server IDENTITY columns)
/// </summary>
public class SampleIdentityProviderLong : IIdentityProvider<long>
{
    private static long _currentUserId;

    public static void SetCurrentUserId(long userId)
    {
        _currentUserId = userId;
    }

    public static void ClearCurrentUserId()
    {
        _currentUserId = default;
    }

    public long GetCurrentUserId()
    {
        return _currentUserId;
    }
}
