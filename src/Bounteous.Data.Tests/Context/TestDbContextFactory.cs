using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Context;

public class TestDbContextFactory : DbContextFactory<TestDbContext, Guid>
{
    public TestDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer, IIdentityProvider<Guid> identityProvider)
        : base(connectionBuilder, observer, identityProvider)
    {
    }

    protected override TestDbContext Create(DbContextOptions applyOptions,
        IDbContextObserver dbContextObserver, IIdentityProvider<Guid> identityProvider)
    {
        return new TestDbContext(applyOptions, dbContextObserver, identityProvider);
    }

    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        if (sensitiveDataLoggingEnabled)
            optionsBuilder.EnableSensitiveDataLogging();

        optionsBuilder.UseInMemoryDatabase("TestDatabase");
        return optionsBuilder.Options;
    }
}