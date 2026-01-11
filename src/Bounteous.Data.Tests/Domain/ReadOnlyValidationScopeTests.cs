using AwesomeAssertions;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Domain;

public class ReadOnlyValidationScopeTests : DbContextTestBase
{
    [Fact]
    public async Task ReadOnlyValidationScope_Allows_Seeding_Test_Data()
    {
        // Arrange
        var product = new ReadOnlyLegacyProduct
        {
            Id = 1000L,
            Name = "Test Product",
            Price = 99.99m,
            Category = "Electronics"
        };

        // Act - Use scope to suppress validation during seeding
        await using var context = CreateContext();
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.Add(product);
            await context.SaveChangesAsync(); // Should succeed
        }
            
        // Assert - Verify data was saved
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(1000L);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Re_Enables_Validation_After_Scope()
    {
        // Arrange
        var product = new ReadOnlyLegacyProduct
        {
            Id = 2000L,
            Name = "Test Product",
            Price = 50.00m,
            Category = "Books"
        };

        await using var context = CreateContext();
        
        // Act - Seed with suppression
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.Add(product);
            await context.SaveChangesAsync(); // Succeeds
        }
        
        // Assert - Validation is re-enabled, modifications should throw
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(2000L);
        saved!.Name = "Modified Name";
        
        var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
            async () => await context.SaveChangesAsync());
        
        exception.Message.Should().Contain("ReadOnlyLegacyProduct");
        exception.Message.Should().Contain("update");
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Can_Seed_Multiple_Entities()
    {
        // Arrange
        var products = new[]
        {
            new ReadOnlyLegacyProduct { Id = 3001L, Name = "Product 1", Price = 10.00m, Category = "A" },
            new ReadOnlyLegacyProduct { Id = 3002L, Name = "Product 2", Price = 20.00m, Category = "B" },
            new ReadOnlyLegacyProduct { Id = 3003L, Name = "Product 3", Price = 30.00m, Category = "C" }
        };

        // Act
        await using var context = CreateContext();
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.AddRange(products);
            await context.SaveChangesAsync();
        }
            
        // Assert
        var count = await context.ReadOnlyLegacyProducts.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Works_With_Mixed_Entity_Types()
    {
        // Arrange
        var readonlyProduct = new ReadOnlyLegacyProduct
        {
            Id = 4000L,
            Name = "Readonly Product",
            Price = 100.00m,
            Category = "Test"
        };
        
        var writableProduct = new LegacyProduct
        {
            Id = 4001L,
            Name = "Writable Product",
            Price = 50.00m
        };

        // Act
        await using var context = CreateContext();
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.Add(readonlyProduct);
            await context.SaveChangesAsync();
        }
            
        // Add writable entity normally (no scope needed)
        context.LegacyProducts.Add(writableProduct);
        await context.SaveChangesAsync(); // Should succeed
            
        // Assert
        var readonlyResult = await context.ReadOnlyLegacyProducts.FindAsync(4000L);
        var writableResult = await context.LegacyProducts.FindAsync(4001L);
            
        readonlyResult.Should().NotBeNull();
        writableResult.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Demonstrates_Documentation_Value()
    {
        await using var context = CreateContext();
        
        // Clear documentation: "I'm seeding test data, validation is suppressed"
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
            {
                Id = 5000L,
                Name = "Seed Data",
                Price = 1.00m,
                Category = "Test"
            });
            await context.SaveChangesAsync();
        }
        
        // Clear documentation: "Validation is active, this should fail"
        var product = await context.ReadOnlyLegacyProducts.FindAsync(5000L);
        product!.Name = "Attempted Modification";
        
        await Assert.ThrowsAsync<ReadOnlyEntityException>(
            () => context.SaveChangesAsync());
    }

    [Fact]
    public async Task WithoutScopeValidationIsEnforcedByDefault()
    {
        await using var context = CreateContext();
        
        // No scope = validation enforced
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 6000L,
            Name = "Should Fail",
            Price = 1.00m,
            Category = "Test"
        });
        
        var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(
            async () => await context.SaveChangesAsync());
        
        exception.Message.Should().Contain("create");
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Can_Be_Nested()
    {
        // Demonstrates that scopes can be nested (though not typically needed)
        await using var context = CreateContext();
        
        using (new ReadOnlyValidationScope())
        {
            // Outer scope suppresses validation
            context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
            {
                Id = 7000L,
                Name = "Outer Scope",
                Price = 10.00m,
                Category = "Test"
            });
            
            using (new ReadOnlyValidationScope())
            {
                // Inner scope also suppresses (redundant but valid)
                context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
                {
                    Id = 7001L,
                    Name = "Inner Scope",
                    Price = 20.00m,
                    Category = "Test"
                });
                
                await context.SaveChangesAsync(); // Both succeed
            }
        }
        
        // Verify both were saved
        var count = await context.ReadOnlyLegacyProducts.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Works_With_Async_Operations()
    {
        // Demonstrates thread-safe async behavior using AsyncLocal
        await using var context = CreateContext();
        
        using (new ReadOnlyValidationScope())
        {
            // Simulate async operation
            await Task.Delay(10);
            
            context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
            {
                Id = 8000L,
                Name = "Async Test",
                Price = 15.00m,
                Category = "Test"
            });
            
            // Another async operation
            await Task.Delay(10);
            
            // Scope should still be active
            await context.SaveChangesAsync(); // Should succeed
        }
        
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(8000L);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadOnlyValidationScope_IsSuppressed_Reflects_Current_State()
    {
        // Demonstrates how to check if validation is currently suppressed
        
        // Initially, validation is NOT suppressed
        ReadOnlyValidationScope.IsSuppressed.Should().BeFalse();
        
        using (new ReadOnlyValidationScope())
        {
            // Inside scope, validation IS suppressed
            ReadOnlyValidationScope.IsSuppressed.Should().BeTrue();
            
            await using var context = CreateContext();
            context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
            {
                Id = 9000L,
                Name = "State Check",
                Price = 5.00m,
                Category = "Test"
            });
            await context.SaveChangesAsync();
        }
        
        // After scope, validation is NOT suppressed again
        ReadOnlyValidationScope.IsSuppressed.Should().BeFalse();
    }

    [Fact]
    public async Task ReadOnlyValidationScope_Usage_Pattern_For_Test_Setup()
    {
        await using var context = CreateContext();
        
        // === ARRANGE: Seed test data with scope ===
        using (new ReadOnlyValidationScope())
        {
            context.ReadOnlyLegacyProducts.AddRange(
                new ReadOnlyLegacyProduct { Id = 10001L, Name = "Product A", Price = 10.00m, Category = "A" },
                new ReadOnlyLegacyProduct { Id = 10002L, Name = "Product B", Price = 20.00m, Category = "B" },
                new ReadOnlyLegacyProduct { Id = 10003L, Name = "Product C", Price = 30.00m, Category = "C" }
            );
            await context.SaveChangesAsync();
        }
        
        // === ACT: Perform test operations (validation active) ===
        var products = await context.ReadOnlyLegacyProducts
            .Where(p => p.Price > 15.00m)
            .ToListAsync();
        
        // === ASSERT: Verify results ===
        products.Count.Should().Be(2);
        products.Should().AllSatisfy(p => p.Price.Should().BeGreaterThan(15.00m));
    }
}
