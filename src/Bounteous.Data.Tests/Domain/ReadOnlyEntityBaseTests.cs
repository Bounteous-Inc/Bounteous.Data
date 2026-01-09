using AwesomeAssertions;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Domain;

/// <summary>
/// Tests for ReadOnlyEntityBase to verify read-only entity behavior.
/// Inherits from DbContextTestBase to reduce setup duplication.
/// </summary>
public class ReadOnlyEntityBaseTests : DbContextTestBase
{

    [Fact]
    public async Task ReadOnlyEntity_Should_Allow_Query()
    {
        // Arrange & Act - Query readonly entities (empty result is fine, we're testing query capability)
        await using (var context = CreateContext())
        {
            var products = await context.ReadOnlyLegacyProducts.ToListAsync();
            
            // Assert - Query should work without throwing
            products.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ReadOnlyEntity_Should_Throw_On_Create()
    {
        // Arrange
        var product = new ReadOnlyLegacyProduct
        {
            Id = 2000L,
            Name = "New Product",
            Price = 50.00m,
            Category = "Books"
        };

        // Act & Assert
        await using (var context = CreateContext())
        {
            context.ReadOnlyLegacyProducts.Add(product);
            
            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
                async () => await context.SaveChangesAsync());
            
            exception.Message.Should().Contain("ReadOnlyLegacyProduct");
            exception.Message.Should().Contain("create");
            exception.Message.Should().Contain("read-only");
        }
    }

    [Fact]
    public async Task ReadOnlyEntity_Should_Throw_On_Update()
    {
        // Arrange
        await using (var context = CreateContext())
        {
            var product = new ReadOnlyLegacyProduct
            {
                Id = 3000L,
                Name = "Original Name",
                Price = 100.00m,
                Category = "Tools"
            };
            
            // Attach as Unchanged to simulate existing entity
            context.Entry(product).State = EntityState.Unchanged;
            
            // Act - Modify the entity
            product.Name = "Modified Name";
            
            // Assert
            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
                async () => await context.SaveChangesAsync());
            
            exception.Message.Should().Contain("ReadOnlyLegacyProduct");
            exception.Message.Should().Contain("update");
        }
    }

    [Fact]
    public async Task ReadOnlyEntity_Should_Throw_On_Delete()
    {
        // Arrange
        await using (var context = CreateContext())
        {
            var product = new ReadOnlyLegacyProduct
            {
                Id = 4000L,
                Name = "To Delete",
                Price = 75.00m,
                Category = "Furniture"
            };
            
            // Attach as Unchanged to simulate existing entity
            context.Entry(product).State = EntityState.Unchanged;
            
            // Act - Delete the entity
            context.ReadOnlyLegacyProducts.Remove(product);
            
            // Assert
            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
                async () => await context.SaveChangesAsync());
            
            exception.Message.Should().Contain("ReadOnlyLegacyProduct");
            exception.Message.Should().Contain("delete");
        }
    }

    [Fact]
    public async Task ReadOnlyEntity_With_Int_Id_Should_Throw_On_Create()
    {
        // Arrange
        var customer = new ReadOnlyLegacyCustomer
        {
            Id = 100,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedDate = DateTime.UtcNow
        };

        // Act & Assert
        await using (var context = CreateContext())
        {
            context.ReadOnlyLegacyCustomers.Add(customer);
            
            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
                async () => await context.SaveChangesAsync());
            
            exception.Message.Should().Contain("ReadOnlyLegacyCustomer");
            exception.Message.Should().Contain("create");
        }
    }

    [Fact]
    public async Task Mixed_ReadOnly_And_Writable_Entities_Should_Work()
    {
        // Arrange
        var writableProduct = new LegacyProduct
        {
            Id = 5000L,
            Name = "Writable Product",
            Price = 25.00m
        };
        
        var readonlyProduct = new ReadOnlyLegacyProduct
        {
            Id = 6000L,
            Name = "Readonly Product",
            Price = 30.00m,
            Category = "Test"
        };

        // Act & Assert - Writable should save, readonly should throw
        await using (var context = CreateContext())
        {
            context.LegacyProducts.Add(writableProduct);
            await context.SaveChangesAsync(); // Should succeed
            
            writableProduct.Id.Should().Be(5000L);
        }

        await using (var context = CreateContext())
        {
            context.ReadOnlyLegacyProducts.Add(readonlyProduct);
            
            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
                async () => await context.SaveChangesAsync());
            
            exception.Message.Should().Contain("ReadOnlyLegacyProduct");
        }
    }

    [Fact]
    public async Task ReadOnlyEntity_Can_Be_Queried_With_Linq()
    {
        // Arrange & Act - Verify LINQ queries work on readonly entities
        await using (var context = CreateContext())
        {
            // Query with filters should not throw
            var query = context.ReadOnlyLegacyProducts
                .Where(p => p.Category == "CategoryA")
                .Where(p => p.Price > 15.00m);
            
            var result = await query.ToListAsync();
            
            // Assert - Query execution should work without throwing
            result.Should().NotBeNull();
        }
    }
}
