using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Data;

public class SampleDbContextFactory : DbContextFactory<SampleDbContext, Guid>
{
    public SampleDbContextFactory(
        IConnectionBuilder connectionBuilder, 
        IDbContextObserver observer, 
        IIdentityProvider<Guid>? identityProvider = null)
        : base(connectionBuilder, observer, identityProvider)
    {
    }

    protected override SampleDbContext Create(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<Guid>? identityProvider)
    {
        return new SampleDbContext(options, observer, identityProvider);
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
