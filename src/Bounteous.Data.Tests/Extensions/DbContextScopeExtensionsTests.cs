using AwesomeAssertions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Extensions;

/// <summary>
/// Tests for DbContext extension methods that provide fluent API for scopes.
/// </summary>
public class DbContextScopeExtensionsTests : DbContextTestBase
{
    [Fact]
    public async Task SuppressReadOnlyValidation_Allows_Seeding_ReadOnly_Entities()
    {
        await using var context = CreateContext();
        
        // Use fluent API from context
        using var scope = context.SuppressReadOnlyValidation();
        
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 1000L,
            Name = "Test Product",
            Price = 99.99m,
            Category = "Electronics"
        });
        
        await context.SaveChangesAsync(); // Should succeed
        
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(1000L);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task SuppressReadOnlyValidation_Returns_Disposable_Scope()
    {
        await using var context = CreateContext();
        
        var scope = context.SuppressReadOnlyValidation();
        
        scope.Should().NotBeNull();
        scope.Should().BeOfType<ReadOnlyValidationScope>();
        
        // Verify it's disposable
        scope.Dispose();
    }

    [Fact]
    public async Task EnforceReadOnly_Blocks_SaveChanges()
    {
        await using var context = CreateContext();
        
        // Use fluent API from context
        using var scope = context.EnforceReadOnly();
        
        context.Customers.Add(new Customer { Name = "Test Customer" });
        
        var act = async () => await context.SaveChangesAsync();
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*read-only request scope*");
    }

    [Fact]
    public async Task EnforceReadOnly_Allows_Query_Operations()
    {
        // Arrange - Seed data
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.AddRange(
                new Customer { Name = "Customer 1" },
                new Customer { Name = "Customer 2" }
            );
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Query with enforced read-only
        await using var context = CreateContext();
        using var scope = context.EnforceReadOnly();
        
        var customers = await context.Customers.ToListAsync();
        var count = await context.Customers.CountAsync();
        
        // Assert
        customers.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task EnforceReadOnly_Returns_Disposable_Scope()
    {
        await using var context = CreateContext();
        
        var scope = context.EnforceReadOnly();
        
        scope.Should().NotBeNull();
        scope.Should().BeOfType<ReadOnlyRequestScope>();
        
        // Verify it's disposable
        scope.Dispose();
    }

    [Fact]
    public async Task AllowTestSeeding_Is_Alias_For_SuppressReadOnlyValidation()
    {
        await using var context = CreateContext();
        
        // Use alias method
        using var scope = context.AllowTestSeeding();
        
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 2000L,
            Name = "Test Product",
            Price = 50m,
            Category = "Test"
        });
        
        await context.SaveChangesAsync(); // Should succeed
        
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(2000L);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task AsQueryOnly_Is_Alias_For_EnforceReadOnly()
    {
        await using var context = CreateContext();
        
        // Use alias method
        using var scope = context.AsQueryOnly();
        
        context.Customers.Add(new Customer { Name = "Test" });
        
        var act = async () => await context.SaveChangesAsync();
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*read-only request scope*");
    }

    [Fact]
    public async Task Fluent_API_Works_With_Using_Declaration()
    {
        await using var context = CreateContext();
        
        // Modern using declaration syntax
        using var scope = context.SuppressReadOnlyValidation();
        
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 3000L,
            Name = "Product",
            Price = 100m,
            Category = "Test"
        });
        
        await context.SaveChangesAsync();
        
        var saved = await context.ReadOnlyLegacyProducts.FindAsync(3000L);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Fluent_API_Intent_Revealing_In_Service_Layer()
    {
        // Simulate a service method using the fluent API
        await using var context = CreateContext();
        
        // Clear intent: "This is a query-only operation"
        using var _ = context.EnforceReadOnly();
        
        // Simulate GET endpoint logic
        var customers = await context.Customers.ToListAsync();
        
        // No SaveChanges - just queries
        customers.Should().NotBeNull();
    }

    [Fact]
    public async Task Fluent_API_Makes_Test_Seeding_Obvious()
    {
        await using var context = CreateContext();
        
        // Clear intent: "I'm seeding test data"
        using var _ = context.SuppressReadOnlyValidation();
        
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 4000L,
            Name = "Test Data",
            Price = 1m,
            Category = "Test"
        });
        
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Multiple_Contexts_Can_Use_Different_Scopes()
    {
        // Test that scopes are isolated per context/async flow
        
        // Context 1: Read-only scope
        await using (var context1 = CreateContext())
        {
            using var readOnlyScope = context1.EnforceReadOnly();
            
            context1.Customers.Add(new Customer { Name = "Test" });
            var act1 = async () => await context1.SaveChangesAsync();
            await act1.Should().ThrowAsync<InvalidOperationException>();
        }
        
        // Context 2: Test seeding scope (separate async context)
        await using (var context2 = CreateContext())
        {
            using var testScope = context2.SuppressReadOnlyValidation();
            
            context2.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
            {
                Id = 5000L,
                Name = "Test",
                Price = 1m,
                Category = "Test"
            });
            await context2.SaveChangesAsync(); // Should succeed
            
            var saved = await context2.ReadOnlyLegacyProducts.FindAsync(5000L);
            saved.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Fluent_API_Discoverable_Through_IntelliSense()
    {
        await using var context = CreateContext();
        
        // These methods should be discoverable when typing "context."
        // Verify they exist and are callable
        var suppressMethod = typeof(DbContextScopeExtensions)
            .GetMethod(nameof(DbContextScopeExtensions.SuppressReadOnlyValidation));
        
        var enforceMethod = typeof(DbContextScopeExtensions)
            .GetMethod(nameof(DbContextScopeExtensions.EnforceReadOnly));
        
        suppressMethod.Should().NotBeNull();
        enforceMethod.Should().NotBeNull();
    }

    [Fact]
    public async Task Scope_Disposal_Works_Correctly_With_Fluent_API()
    {
        await using var context = CreateContext();
        
        // Use scope
        using (var scope = context.SuppressReadOnlyValidation())
        {
            ReadOnlyValidationScope.IsSuppressed.Should().BeTrue();
        }
        
        // After disposal
        ReadOnlyValidationScope.IsSuppressed.Should().BeFalse();
        
        // Use another scope
        using (var scope = context.EnforceReadOnly())
        {
            ReadOnlyRequestScope.IsActive.Should().BeTrue();
        }
        
        // After disposal
        ReadOnlyRequestScope.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Fluent_API_Works_In_Realistic_Service_Pattern()
    {
        // Simulate a realistic service class pattern
        async Task<List<Customer>> GetCustomersAsync()
        {
            await using var context = CreateContext();
            using var _ = context.EnforceReadOnly();
            
            return await context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        
        // Should work without errors
        var customers = await GetCustomersAsync();
        customers.Should().NotBeNull();
    }

    [Fact]
    public async Task Fluent_API_Reduces_Boilerplate_Compared_To_Direct_Scope()
    {
        await using var context = CreateContext();
        
        // OLD WAY (more verbose):
        // using (new ReadOnlyValidationScope())
        // {
        //     // ...
        // }
        
        // NEW WAY (more concise and discoverable):
        using var scope = context.SuppressReadOnlyValidation();
        
        // Both work the same, but fluent API is:
        // 1. More discoverable (IntelliSense on context)
        // 2. More intent-revealing (method name explains purpose)
        // 3. Tied to context (makes relationship clear)
        
        scope.Should().NotBeNull();
    }
}
