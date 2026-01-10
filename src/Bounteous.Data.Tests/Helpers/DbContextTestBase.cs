using Bounteous.Data.Tests.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests.Helpers;

/// <summary>
/// Base class for tests that require DbContext setup.
/// Provides common infrastructure while keeping individual tests readable.
/// </summary>
public abstract class DbContextTestBase : IDisposable
{
    protected DbContextOptions<TestDbContext> DbContextOptions { get; }
    protected Mock<IDbContextObserver> MockObserver { get; }
    protected TestIdentityProvider<Guid> IdentityProvider { get; }

    protected DbContextTestBase()
    {
        DbContextOptions = TestDbContextFactory.CreateOptions();
        MockObserver = MockObserverFactory.CreateLooseMock();
        IdentityProvider = new TestIdentityProvider<Guid>();
    }

    /// <summary>
    /// Creates a TestDbContext with the configured options.
    /// </summary>
    protected TestDbContext CreateContext()
    {
        return new TestDbContext(DbContextOptions, MockObserver.Object, IdentityProvider);
    }

    /// <summary>
    /// Creates a TestDbContext with a specific user ID set.
    /// </summary>
    protected TestDbContext CreateContextWithUserId(Guid userId)
    {
        IdentityProvider.SetCurrentUserId(userId);
        return CreateContext();
    }

    public virtual void Dispose()
    {
        // Override in derived classes if cleanup is needed
    }
}

/// <summary>
/// Base class for tests that require strict mock verification.
/// </summary>
public abstract class DbContextTestBaseWithStrictMocks : IDisposable
{
    protected MockRepository MockRepository { get; }
    protected DbContextOptions<TestDbContext> DbContextOptions { get; }
    protected Mock<IDbContextObserver> MockObserver { get; }
    protected TestIdentityProvider<Guid> IdentityProvider { get; }

    protected DbContextTestBaseWithStrictMocks()
    {
        MockRepository = new MockRepository(MockBehavior.Strict);
        DbContextOptions = TestDbContextFactory.CreateOptions();
        MockObserver = MockObserverFactory.CreateStrictMock(MockRepository);
        IdentityProvider = new TestIdentityProvider<Guid>();
    }

    /// <summary>
    /// Creates a TestDbContext with the configured options.
    /// </summary>
    protected TestDbContext CreateContext()
    {
        return new TestDbContext(DbContextOptions, MockObserver.Object, IdentityProvider);
    }

    public virtual void Dispose()
    {
        MockRepository.VerifyAll();
    }
}
