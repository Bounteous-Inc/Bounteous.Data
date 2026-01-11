using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Domain;

public class ReadOnlyRequestScopeTests : DbContextTestBase
{
    [Fact]
    public async Task ReadOnlyRequestScope_Blocks_SaveChanges_With_No_Modifications()
    {
        await using var context = CreateContext();
        
        using (new ReadOnlyRequestScope())
        {
            // Even with no modifications, SaveChanges should throw
            var act = async () => await context.SaveChangesAsync();
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Blocks_SaveChanges_With_Added_Entity()
    {
        await using var context = CreateContext();
        
        using (new ReadOnlyRequestScope())
        {
            // Add an entity
            context.Customers.Add(new Customer { Name = "Test Customer" });
            
            // SaveChanges should throw with entity details
            var act = async () => await context.SaveChangesAsync();
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*")
                .WithMessage("*Customer*")
                .WithMessage("*Added*");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Blocks_SaveChanges_With_Modified_Entity()
    {
        // Arrange - Create entity outside scope
        var customerId = Guid.NewGuid();
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.Add(new Customer { Id = customerId, Name = "Original Name" });
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Try to modify within scope
        await using var context = CreateContext();
        using (new ReadOnlyRequestScope())
        {
            var customer = await context.Customers.FindAsync(customerId);
            customer!.Name = "Modified Name";
            
            var act = async () => await context.SaveChangesAsync();
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*")
                .WithMessage("*Customer*")
                .WithMessage("*Modified*");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Blocks_SaveChanges_With_Deleted_Entity()
    {
        // Arrange - Create entity outside scope
        var customerId = Guid.NewGuid();
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.Add(new Customer { Id = customerId, Name = "To Delete" });
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Try to delete within scope
        await using var context = CreateContext();
        using (new ReadOnlyRequestScope())
        {
            var customer = await context.Customers.FindAsync(customerId);
            context.Customers.Remove(customer!);
            
            var act = async () => await context.SaveChangesAsync();
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*")
                .WithMessage("*Customer*")
                .WithMessage("*Deleted*");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Allows_Query_Operations()
    {
        // Arrange - Create test data
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.AddRange(
                new Customer { Name = "Customer 1" },
                new Customer { Name = "Customer 2" },
                new Customer { Name = "Customer 3" }
            );
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Query within scope (should work)
        await using var context = CreateContext();
        using (new ReadOnlyRequestScope())
        {
            var customers = await context.Customers.ToListAsync();
            var count = await context.Customers.CountAsync();
            var firstCustomer = await context.Customers.FirstOrDefaultAsync();
            var filteredCustomers = await context.Customers
                .Where(c => c.Name.Contains("Customer"))
                .ToListAsync();
            
            // Assert - All queries should succeed
            customers.Should().HaveCount(3);
            count.Should().Be(3);
            firstCustomer.Should().NotBeNull();
            filteredCustomers.Should().HaveCount(3);
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Allows_SaveChanges_After_Disposal()
    {
        await using var context = CreateContext();
        
        // Use scope and dispose
        using (new ReadOnlyRequestScope())
        {
            // Scope is active here
            ReadOnlyRequestScope.IsActive.Should().BeTrue();
        }
        
        // After disposal, SaveChanges should work
        ReadOnlyRequestScope.IsActive.Should().BeFalse();
        context.Customers.Add(new Customer { Name = "Test Customer" });
        await context.SaveChangesAsync(); // Should succeed
        
        var count = await context.Customers.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task ReadOnlyRequestScope_IsActive_Reflects_Current_State()
    {
        // Initially not active
        ReadOnlyRequestScope.IsActive.Should().BeFalse();
        
        using (new ReadOnlyRequestScope())
        {
            // Active within scope
            ReadOnlyRequestScope.IsActive.Should().BeTrue();
        }
        
        // Not active after disposal
        ReadOnlyRequestScope.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Works_With_Nested_Scopes()
    {
        await using var context = CreateContext();
        
        using (new ReadOnlyRequestScope())
        {
            ReadOnlyRequestScope.IsActive.Should().BeTrue();
            
            using (new ReadOnlyRequestScope())
            {
                // Still active in nested scope
                ReadOnlyRequestScope.IsActive.Should().BeTrue();
                
                context.Customers.Add(new Customer { Name = "Test" });
                var act = async () => await context.SaveChangesAsync();
                await act.Should().ThrowAsync<InvalidOperationException>();
            }
            
            // Inner scope disposed, but outer still active
            ReadOnlyRequestScope.IsActive.Should().BeFalse(); // AsyncLocal resets on inner dispose
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Typical_Query_Workflow()
    {
        // Arrange - Seed data
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.AddRange(
                new Customer { Name = "Acme Corp" },
                new Customer { Name = "TechStart Inc" },
                new Customer { Name = "Global Solutions" }
            );
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Simulate a GET endpoint for listing companies
        List<Customer> companies;
        await using (var context = CreateContext())
        {
            using (new ReadOnlyRequestScope())
            {
                // This represents a typical query-only operation
                companies = await context.Customers
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                // No SaveChanges call - this is query-only
            }
        }
        
        // Assert
        companies.Should().HaveCount(3);
        companies[0].Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Prevents_Accidental_Modifications()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.Add(new Customer { Id = customerId, Name = "Original" });
            await setupContext.SaveChangesAsync();
        }
        
        // Act - Simulate a developer accidentally modifying data in a query endpoint
        await using var context = CreateContext();
        using (new ReadOnlyRequestScope())
        {
            var customer = await context.Customers.FindAsync(customerId);
            
            // Accidental modification (bug in code)
            customer!.Name = "Accidentally Modified";
            
            // The scope catches this mistake
            var act = async () => await context.SaveChangesAsync();
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*");
        }
        
        // Verify data wasn't changed
        await using (var verifyContext = CreateContext())
        {
            var customer = await verifyContext.Customers.FindAsync(customerId);
            customer!.Name.Should().Be("Original");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Works_With_Multiple_Entity_Types()
    {
        await using var context = CreateContext();
        
        using (new ReadOnlyRequestScope())
        {
            context.Customers.Add(new Customer { Name = "Customer" });
            context.LegacyProducts.Add(new LegacyProduct { Id = 1L, Name = "Product", Price = 10m });
            
            var act = async () => await context.SaveChangesAsync();
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Customer*")
                .WithMessage("*LegacyProduct*");
        }
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Thread_Safe_With_AsyncLocal()
    {
        // Test that scope is isolated per async context
        var task1 = Task.Run(async () =>
        {
            using (new ReadOnlyRequestScope())
            {
                await Task.Delay(50);
                ReadOnlyRequestScope.IsActive.Should().BeTrue();
            }
        });
        
        var task2 = Task.Run(async () =>
        {
            await Task.Delay(25);
            // Should not be affected by task1's scope
            ReadOnlyRequestScope.IsActive.Should().BeFalse();
        });
        
        await Task.WhenAll(task1, task2);
    }
}
