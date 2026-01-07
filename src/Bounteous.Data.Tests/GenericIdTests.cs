using AwesomeAssertions;
using Bounteous.Core.Time;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

namespace Bounteous.Data.Tests;

public class GenericIdTests
{
    private readonly DbContextOptions<DbContextBase> dbContextOptions;
    private readonly Mock<IDbContextObserver> mockObserver;

    public GenericIdTests()
    {
        dbContextOptions = new DbContextOptionsBuilder<DbContextBase>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;
        
        mockObserver = new Mock<IDbContextObserver>(MockBehavior.Loose);
    }

    [Fact]
    public async Task GuidBasedEntity_Should_Work_As_Before()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Assert
        customer.Id.Should().NotBe(Guid.Empty);
        customer.CreatedOn.Should().NotBe(default);
        customer.ModifiedOn.Should().NotBe(default);
    }

    [Fact]
    public async Task LongBasedEntity_With_Audit_Should_Save_Successfully()
    {
        // Arrange
        var product = new LegacyProduct 
        { 
            Id = 12345L,
            Name = "Legacy Product",
            Price = 99.99m
        };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyProducts.Add(product);
            await context.SaveChangesAsync();
        }

        // Assert
        product.Id.Should().Be(12345L);
        product.CreatedOn.Should().NotBe(default);
        product.ModifiedOn.Should().NotBe(default);
        
        // Verify it was saved
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var retrieved = await context.LegacyProducts.FindAsync(12345L);
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Legacy Product");
            retrieved.Price.Should().Be(99.99m);
        }
    }

    [Fact]
    public async Task IntBasedEntity_With_Audit_Should_Save_Successfully()
    {
        // Arrange
        var order = new LegacyOrder
        {
            Id = 999,
            CustomerId = 123L,
            OrderNumber = "ORD-999",
            TotalAmount = 250.50m
        };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyOrders.Add(order);
            await context.SaveChangesAsync();
        }

        // Assert
        order.Id.Should().Be(999);
        order.CreatedOn.Should().NotBe(default);
        order.ModifiedOn.Should().NotBe(default);
        
        // Verify it was saved
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var retrieved = await context.LegacyOrders.FindAsync(999);
            retrieved.Should().NotBeNull();
            retrieved!.OrderNumber.Should().Be("ORD-999");
        }
    }

    [Fact]
    public async Task LongBasedEntity_Without_Audit_Should_Save_Successfully()
    {
        // Arrange
        var category = new LegacyCategory
        {
            Id = 100L,
            Name = "Electronics",
            Description = "Electronic devices"
        };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyCategories.Add(category);
            await context.SaveChangesAsync();
        }

        // Assert
        category.Id.Should().Be(100L);
        
        // Verify it was saved
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var retrieved = await context.LegacyCategories.FindAsync(100L);
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Electronics");
            retrieved.Description.Should().Be("Electronic devices");
        }
    }

    [Fact]
    public async Task FindById_Should_Work_With_Long_Id()
    {
        // Arrange
        var product = new LegacyProduct 
        { 
            Id = 54321L,
            Name = "Findable Product",
            Price = 49.99m
        };

        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyProducts.Add(product);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var found = await context.LegacyProducts.FindById<LegacyProduct, long>(54321L);
            
            // Assert
            found.Should().NotBeNull();
            found.Name.Should().Be("Findable Product");
            found.Price.Should().Be(49.99m);
        }
    }

    [Fact]
    public async Task FindById_Should_Work_With_Int_Id()
    {
        // Arrange
        var order = new LegacyOrder
        {
            Id = 777,
            CustomerId = 456L,
            OrderNumber = "ORD-777",
            TotalAmount = 150.00m
        };

        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyOrders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var found = await context.LegacyOrders.FindById<LegacyOrder, int>(777);
            
            // Assert
            found.Should().NotBeNull();
            found.OrderNumber.Should().Be("ORD-777");
            found.TotalAmount.Should().Be(150.00m);
        }
    }

    [Fact]
    public async Task FindById_Should_Throw_NotFoundException_For_Long_Id()
    {
        // Arrange - Use a separate mock that doesn't require SaveChanges
        var mockObs = new Mock<IDbContextObserver>(MockBehavior.Loose);
        
        var options = new DbContextOptionsBuilder<DbContextBase>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        await using (var context = new TestDbContext(options, mockObs.Object))
        {
            var exception = await Assert.ThrowsAsync<NotFoundException<LegacyProduct, long>>(
                async () => await context.LegacyProducts.FindById<LegacyProduct, long>(99999L));
            
            exception.Message.Should().Contain("LegacyProduct");
            exception.Message.Should().Contain("99999");
        }
    }

    [Fact]
    public async Task FindById_Should_Throw_NotFoundException_For_Int_Id()
    {
        // Arrange - Use a separate mock that doesn't require SaveChanges
        var mockObs = new Mock<IDbContextObserver>(MockBehavior.Loose);
        
        var options = new DbContextOptionsBuilder<DbContextBase>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        await using (var context = new TestDbContext(options, mockObs.Object))
        {
            var exception = await Assert.ThrowsAsync<NotFoundException<LegacyOrder, int>>(
                async () => await context.LegacyOrders.FindById<LegacyOrder, int>(88888));
            
            exception.Message.Should().Contain("LegacyOrder");
            exception.Message.Should().Contain("88888");
        }
    }

    [Fact]
    public async Task Mixed_Id_Strategies_Should_Work_In_Same_Context()
    {
        // Arrange
        var customer = new Customer { Name = "Mixed Customer" };
        var product = new LegacyProduct { Id = 11111L, Name = "Mixed Product", Price = 25.00m };
        var order = new LegacyOrder { Id = 555, CustomerId = 789L, OrderNumber = "MIX-555", TotalAmount = 75.00m };
        var category = new LegacyCategory { Id = 200L, Name = "Mixed Category" };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            context.LegacyProducts.Add(product);
            context.LegacyOrders.Add(order);
            context.LegacyCategories.Add(category);
            await context.SaveChangesAsync();
        }

        // Assert - Verify all entities were saved with correct IDs
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var retrievedCustomer = await context.Customers.FindAsync(customer.Id);
            var retrievedProduct = await context.LegacyProducts.FindAsync(11111L);
            var retrievedOrder = await context.LegacyOrders.FindAsync(555);
            var retrievedCategory = await context.LegacyCategories.FindAsync(200L);

            retrievedCustomer.Should().NotBeNull();
            retrievedCustomer!.Id.Should().Be(customer.Id);
            
            retrievedProduct.Should().NotBeNull();
            retrievedProduct!.Id.Should().Be(11111L);
            
            retrievedOrder.Should().NotBeNull();
            retrievedOrder!.Id.Should().Be(555);
            
            retrievedCategory.Should().NotBeNull();
            retrievedCategory!.Id.Should().Be(200L);
        }
    }

    [Fact]
    public async Task Audit_Fields_Should_Be_Set_For_Long_Id_Entity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = new LegacyProduct 
        { 
            Id = 22222L,
            Name = "Audit Test Product",
            Price = 100.00m
        };

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.WithUserId(userId);
            context.LegacyProducts.Add(product);
            await context.SaveChangesAsync();
        }

        // Assert
        product.CreatedOn.Should().NotBe(default);
        product.ModifiedOn.Should().NotBe(default);
        product.CreatedBy.Should().Be(userId);
        product.ModifiedBy.Should().Be(userId);
        product.Version.Should().Be(1);
        product.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Audit_Fields_Should_Be_Updated_For_Int_Id_Entity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var order = new LegacyOrder
        {
            Id = 333,
            CustomerId = 111L,
            OrderNumber = "ORD-333",
            TotalAmount = 50.00m
        };

        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.WithUserId(userId);
            context.LegacyOrders.Add(order);
            await context.SaveChangesAsync();
        }

        var originalModifiedOn = order.ModifiedOn;

        // Act - Modify the entity
        await Task.Delay(100); // Ensure time difference
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var modifiedUserId = Guid.NewGuid();
            context.WithUserId(modifiedUserId);
            
            var retrieved = await context.LegacyOrders.FindAsync(333);
            retrieved!.TotalAmount = 75.00m;
            await context.SaveChangesAsync();

            // Assert
            retrieved.ModifiedOn.Should().BeOnOrAfter(originalModifiedOn);
            retrieved.ModifiedBy.Should().Be(modifiedUserId);
            retrieved.Version.Should().Be(2);
        }
    }

    [Fact]
    public async Task Soft_Delete_Should_Work_For_Long_Id_Entity()
    {
        // Arrange
        var product = new LegacyProduct 
        { 
            Id = 44444L,
            Name = "Delete Test Product",
            Price = 200.00m
        };

        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.LegacyProducts.Add(product);
            await context.SaveChangesAsync();
        }

        // Act - Delete the entity
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var retrieved = await context.LegacyProducts.FindAsync(44444L);
            if (retrieved != null)
            {
                context.LegacyProducts.Remove(retrieved);
                await context.SaveChangesAsync();

                // Assert
                retrieved.IsDeleted.Should().BeTrue();
                retrieved.ModifiedOn.Should().NotBe(default);
            }
        }
    }

}
