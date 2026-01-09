using Moq;

namespace Bounteous.Data.Tests.Helpers;

/// <summary>
/// Factory for creating mock IDbContextObserver instances with common configurations.
/// Reduces duplication in test setup while maintaining clarity.
/// </summary>
public static class MockObserverFactory
{
    /// <summary>
    /// Creates a loose mock observer that allows any calls without explicit setup.
    /// Use this for tests that don't need to verify observer interactions.
    /// </summary>
    public static Mock<IDbContextObserver> CreateLooseMock()
    {
        return new Mock<IDbContextObserver>(MockBehavior.Loose);
    }

    /// <summary>
    /// Creates a strict mock observer with common setup for OnSaved and Dispose.
    /// Use this for tests that need to verify specific observer interactions.
    /// </summary>
    public static Mock<IDbContextObserver> CreateStrictMock(MockRepository? repository = null)
    {
        var mockRepo = repository ?? new MockRepository(MockBehavior.Strict);
        var mock = mockRepo.Create<IDbContextObserver>();
        
        mock.Setup(x => x.OnSaved());
        mock.Setup(x => x.Dispose());
        
        return mock;
    }

    /// <summary>
    /// Creates a strict mock observer with full setup for all methods.
    /// Use this for tests that need complete control over observer behavior.
    /// </summary>
    public static Mock<IDbContextObserver> CreateFullyConfiguredMock(MockRepository? repository = null)
    {
        var mockRepo = repository ?? new MockRepository(MockBehavior.Strict);
        var mock = mockRepo.Create<IDbContextObserver>();
        
        mock.Setup(x => x.OnEntityTracked(It.IsAny<object>(), It.IsAny<Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs>()));
        mock.Setup(x => x.OnStateChanged(It.IsAny<object>(), It.IsAny<Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs>()));
        mock.Setup(x => x.OnSaved());
        mock.Setup(x => x.Dispose());
        
        return mock;
    }
}
