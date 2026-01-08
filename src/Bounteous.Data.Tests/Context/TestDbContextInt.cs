using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Context;

public class TestDbContextInt : DbContextBase<int>
{
    public TestDbContextInt(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<ProductWithIntUserId> Products { get; set; } = null!;

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductWithIntUserId>();
    }
}
