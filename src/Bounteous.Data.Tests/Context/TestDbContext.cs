using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Context;

public class TestDbContext : DbContextBase<Guid>, IDbContext<Guid>
{
    public TestDbContext(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public TestDbContext(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<Guid>? identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    public DbSet<LegacyOrder> LegacyOrders { get; set; }
    public DbSet<LegacyCategory> LegacyCategories { get; set; }
    public DbSet<ReadOnlyLegacyProduct> ReadOnlyLegacyProducts { get; set; }
    public DbSet<ReadOnlyLegacyCustomer> ReadOnlyLegacyCustomers { get; set; }

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyOrder>()
            .Property(o => o.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyCategory>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<ReadOnlyLegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<ReadOnlyLegacyCustomer>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
    }
}