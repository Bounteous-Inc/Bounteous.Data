using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Context;

public class TestDbContextLong : DbContextBase<long>
{
    public TestDbContextLong(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<ProductWithLongUserId> Products { get; set; } = null!;
    public DbSet<CustomerWithLongUserId> Customers { get; set; } = null!;

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductWithLongUserId>();
        modelBuilder.Entity<CustomerWithLongUserId>();
    }
}
