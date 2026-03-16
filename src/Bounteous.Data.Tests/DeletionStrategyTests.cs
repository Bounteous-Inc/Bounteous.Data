using AwesomeAssertions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests;

public class DeletionStrategyTests : IDisposable
{
    private readonly DbContextOptions<TestDbContextLong> dbContextOptions;
    private readonly TestIdentityProvider<long> identityProvider;

    public DeletionStrategyTests()
    {
        dbContextOptions = new DbContextOptionsBuilder<TestDbContextLong>()
            .UseInMemoryDatabase(databaseName: $"DeletionTestDb_{Guid.NewGuid()}")
            .Options;
        
        identityProvider = new TestIdentityProvider<long>();
    }

    [Fact]
    public async Task ISoftDelete_Entity_Should_Be_Soft_Deleted_By_Default()
    {
        // Arrange
        var userId = 12345L;
        var product = new ProductWithLongUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act - Create
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Act - Soft Delete
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var existingProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            context.Products.Remove(existingProduct);
            await context.SaveChangesAsync();
        }

        // Assert - Product is soft deleted
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            // Should not appear in normal queries
            var normalQuery = await context.Products
                .Where(p => p.Id == product.Id)
                .FirstOrDefaultAsync();
            normalQuery.Should().BeNull();

            // Should appear when ignoring query filters
            var withDeleted = await context.Products
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == product.Id);
            
            withDeleted.IsDeleted.Should().BeTrue();
            withDeleted.ModifiedBy.Should().Be(userId);
        }
    }

    [Fact]
    public async Task IHardDelete_Entity_Should_Be_Physically_Deleted()
    {
        // Arrange
        var userId = 12345L;
        var log = new TempLog
        {
            Message = "Test log entry",
            LoggedAt = DateTime.UtcNow
        };

        // Act - Create
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.TempLogs.Add(log);
            await context.SaveChangesAsync();
        }

        // Act - Hard Delete
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var existingLog = await context.TempLogs.FirstAsync(l => l.Id == log.Id);
            context.TempLogs.Remove(existingLog);
            await context.SaveChangesAsync();
        }

        // Assert - Log is physically deleted
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            var deletedLog = await context.TempLogs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == log.Id);
            
            deletedLog.Should().BeNull("entity should be physically deleted from database");
        }
    }

    [Fact]
    public async Task ISoftDelete_Entity_Can_Be_Force_Deleted_With_Delete_Method()
    {
        // Arrange
        var userId = 12345L;
        var product = new ProductWithLongUserId
        {
            Name = "Test Product for GDPR",
            Description = "Must be deleted for compliance",
            Price = 49.99m
        };

        // Act - Create
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Act - Force Physical Delete (GDPR)
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var existingProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            context.Products.Delete(existingProduct);
            await context.SaveChangesAsync();
        }

        // Assert - Product is physically deleted
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            var deletedProduct = await context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == product.Id);
            
            deletedProduct.Should().BeNull("entity should be physically deleted when using Delete() method");
        }
    }

    [Fact]
    public async Task IncludeDeleted_Should_Return_Soft_Deleted_Entities()
    {
        // Arrange
        var userId = 12345L;
        var activeProduct = new ProductWithLongUserId { Name = "Active", Price = 10m };
        var deletedProduct = new ProductWithLongUserId { Name = "Deleted", Price = 20m };

        // Act - Create both products
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Products.AddRange(activeProduct, deletedProduct);
            await context.SaveChangesAsync();
        }

        // Act - Soft delete one product
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var toDelete = await context.Products.FirstAsync(p => p.Id == deletedProduct.Id);
            context.Products.Remove(toDelete);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            // Normal query - only active
            var normalProducts = await context.Products.ToListAsync();
            normalProducts.Should().HaveCount(1);
            normalProducts.First().Name.Should().Be("Active");

            // With IncludeDeleted - both products
            var allProducts = await context.Products.IncludeDeleted().ToListAsync();
            allProducts.Should().HaveCount(2);
            allProducts.Should().Contain(p => p.Name == "Active");
            allProducts.Should().Contain(p => p.Name == "Deleted" && p.IsDeleted);
        }
    }

    [Fact]
    public async Task DeleteRange_Should_Force_Delete_Multiple_ISoftDelete_Entities()
    {
        // Arrange
        var userId = 12345L;
        var products = new[]
        {
            new ProductWithLongUserId { Name = "Product 1", Price = 10m },
            new ProductWithLongUserId { Name = "Product 2", Price = 20m },
            new ProductWithLongUserId { Name = "Product 3", Price = 30m }
        };

        // Act - Create products
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // Act - Force delete all
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var toDelete = await context.Products
                .Where(p => products.Select(x => x.Id).Contains(p.Id))
                .ToListAsync();
            context.Products.DeleteRange(toDelete);
            await context.SaveChangesAsync();
        }

        // Assert - All physically deleted
        await using (var context = new TestDbContextLong(dbContextOptions, null, identityProvider))
        {
            var remaining = await context.Products
                .IgnoreQueryFilters()
                .Where(p => products.Select(x => x.Id).Contains(p.Id))
                .ToListAsync();
            
            remaining.Should().BeEmpty("all entities should be physically deleted");
        }
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
