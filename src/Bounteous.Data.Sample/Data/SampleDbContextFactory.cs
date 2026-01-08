using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Data;

public class SampleDbContextFactory : DbContextFactory<SampleDbContext, Guid>
{
    public SampleDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override SampleDbContext Create(DbContextOptions options, IDbContextObserver observer)
    {
        return new SampleDbContext(options, observer);
    }

    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
        
        if (sensitiveDataLoggingEnabled)
            optionsBuilder.EnableSensitiveDataLogging();

        optionsBuilder.UseInMemoryDatabase("SampleDatabase");
        
        return optionsBuilder.Options;
    }
}
