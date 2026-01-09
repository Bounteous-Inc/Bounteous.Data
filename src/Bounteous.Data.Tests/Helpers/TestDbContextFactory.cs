using Bounteous.Data.Tests.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests.Helpers;

/// <summary>
/// Factory for creating test database contexts with consistent configuration.
/// Reduces duplication in test setup while maintaining clarity.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates DbContextOptions for TestDbContext with a unique in-memory database.
    /// </summary>
    public static DbContextOptions<TestDbContext> CreateOptions(string? databaseName = null)
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"TestDatabase_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Creates DbContextOptions for TestDbContextInt with a unique in-memory database.
    /// </summary>
    public static DbContextOptions<TestDbContextInt> CreateOptionsInt(string? databaseName = null)
    {
        return new DbContextOptionsBuilder<TestDbContextInt>()
            .UseInMemoryDatabase(databaseName ?? $"TestDatabaseInt_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Creates DbContextOptions for TestDbContextLong with a unique in-memory database.
    /// </summary>
    public static DbContextOptions<TestDbContextLong> CreateOptionsLong(string? databaseName = null)
    {
        return new DbContextOptionsBuilder<TestDbContextLong>()
            .UseInMemoryDatabase(databaseName ?? $"TestDatabaseLong_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Creates DbContextOptions for DbContextBase with a unique in-memory database.
    /// </summary>
    public static DbContextOptions<DbContextBase<TUserId>> CreateOptions<TUserId>(string? databaseName = null)
        where TUserId : struct
    {
        return new DbContextOptionsBuilder<DbContextBase<TUserId>>()
            .UseInMemoryDatabase(databaseName ?? $"TestDatabase_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Creates a TestDbContext with standard test configuration.
    /// </summary>
    public static TestDbContext CreateContext(
        DbContextOptions<TestDbContext>? options = null,
        IDbContextObserver? observer = null,
        TestIdentityProvider<Guid>? identityProvider = null)
    {
        return new TestDbContext(
            options ?? CreateOptions(),
            observer ?? new Mock<IDbContextObserver>(MockBehavior.Loose).Object,
            identityProvider ?? new TestIdentityProvider<Guid>());
    }
}
