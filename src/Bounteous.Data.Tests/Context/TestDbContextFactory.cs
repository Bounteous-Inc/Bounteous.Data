using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Context;

public class TestDbContextFactory : DbContextFactory<TestDbContext>
{
    public TestDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override TestDbContext Create(DbContextOptions applyOptions,
        IDbContextObserver dbContextObserver)
    {
        return new TestDbContext(applyOptions, dbContextObserver);
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