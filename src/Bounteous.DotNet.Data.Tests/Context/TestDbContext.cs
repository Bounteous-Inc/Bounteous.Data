using Bounteous.DotNet.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.DotNet.Data.Tests.Context;

public class TestDbContext : DbContextBase
{
    public TestDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        //do nothing
    }
}