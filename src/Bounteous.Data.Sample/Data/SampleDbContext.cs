using Bounteous.Data;
using Bounteous.Data.Converters;
using Bounteous.Data.Domain.ReadOnly;
using Bounteous.Data.Sample.Domain.Entities;
using Bounteous.Data.Sample.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Sample.Data;

public class SampleDbContext : DbContextBase<Guid>
{
    public SampleDbContext(
        DbContextOptions options, 
        IDbContextObserver observer, 
        IIdentityProvider<Guid> identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    
    // ReadOnlyDbSet example - returns wrapped DbSet that throws immediately on write operations
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Sku).IsRequired();
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
            entity.HasIndex(e => e.Sku).IsUnique();
            
            // Demonstrate EnumConverter - stores enum as description string
            entity.Property(e => e.Status)
                .HasConversion(new EnumConverter<ProductStatus>());
            
            // Demonstrate DateTimeConverter - ensures UTC storage
            entity.Property(e => e.LastRestockedOn)
                .HasConversion(new DateTimeConverter());
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired();
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);
            
            entity.HasMany(e => e.OrderItems)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2);
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        modelBuilder.Entity<LegacySystem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SystemName).IsRequired();
            entity.ToTable("LegacySystems");
        });
    }
}
