using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests;

/// <summary>
/// Tests validating the fluent interface pattern for DbContextBase.
/// Ensures that WithUserId returns the correct IDbContext instance for method chaining.
/// </summary>
public class DbContextFluentInterfaceTests
{
    [Fact]
    public void WithUserId_Should_Return_IDbContext_Interface()
    {
        // Arrange
        var options = Helpers.TestDbContextFactory.CreateOptions();
        var identityProvider = new TestIdentityProvider<Guid>();
        
        using var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider);
        var userId = Guid.NewGuid();
        
        // Act - Call WithUserId and capture the return value
        var result = ((IDbContext<Guid>)context).WithUserId(userId);
        
        // Assert - Verify the return type is IDbContext<Guid>
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IDbContext<Guid>>();
    }
    
    [Fact]
    public void WithUserId_Should_Return_Same_Context_Instance()
    {
        // Arrange
        var options = Helpers.TestDbContextFactory.CreateOptions();
        var identityProvider = new TestIdentityProvider<Guid>();
        
        using var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider);
        var userId = Guid.NewGuid();
        
        // Act - Call WithUserId
        var result = ((IDbContext<Guid>)context).WithUserId(userId);
        
        // Assert - Verify it returns the same context instance (for fluent chaining)
        result.Should().BeSameAs(context);
    }
    
    [Fact]
    public void WithUserId_Should_Support_Fluent_Chaining()
    {
        // Arrange
        var options = Helpers.TestDbContextFactory.CreateOptions();
        var identityProvider = new TestIdentityProvider<Guid>();
        
        using var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider);
        var userId = Guid.NewGuid();
        
        // Act - Demonstrate fluent chaining (though WithUserId is the only chainable method currently)
        var result = ((IDbContext<Guid>)context)
            .WithUserId(userId)
            .WithUserId(userId); // Can be called multiple times
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(context);
    }
    
    [Fact]
    public async Task WithUserId_Fluent_Interface_Works_In_Real_Scenario()
    {
        // Arrange
        var options = Helpers.TestDbContextFactory.CreateOptions();
        var identityProvider = new TestIdentityProvider<Guid>();
        var userId = Guid.NewGuid();
        
        // Act - Use fluent interface in a realistic scenario
        await using (var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider))
        {
            // Fluent usage: Set user ID and immediately add entity
            ((IDbContext<Guid>)context)
                .WithUserId(userId);
            
            context.Customers.Add(new Customer
            {
                Name = "Test Customer"
            });
            
            await context.SaveChangesAsync();
        }
        
        // Assert - Verify the entity was saved with the correct user ID
        await using (var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider))
        {
            var customer = await context.Customers.FirstOrDefaultAsync();
            customer.Should().NotBeNull();
            customer!.CreatedBy.Should().Be(userId);
            customer.ModifiedBy.Should().Be(userId);
        }
    }
    
    [Fact]
    public void WithUserId_Return_Value_Should_Be_Used_By_Rider()
    {
        // This test documents that the return value IS used and Rider's warning is incorrect
        // The fluent interface pattern requires returning IDbContext<TUserId> for method chaining
        
        // Arrange
        var options = Helpers.TestDbContextFactory.CreateOptions();
        var identityProvider = new TestIdentityProvider<Guid>();
        
        using var context = Helpers.TestDbContextFactory.CreateContext(options, null, identityProvider);
        var userId = Guid.NewGuid();
        
        // Act - Explicitly use the return value (this is the intended pattern)
        IDbContext<Guid> fluentContext = ((IDbContext<Guid>)context).WithUserId(userId);
        
        // Assert - The return value enables fluent chaining
        fluentContext.Should().NotBeNull();
        fluentContext.Should().BeAssignableTo<IDbContext<Guid>>();
        
        // This pattern allows for:
        // var result = context.WithUserId(userId).SomeFutureMethod().AnotherMethod();
    }
    
    [Fact]
    public void WithUserId_Long_Should_Return_IDbContext_Interface()
    {
        // Test with long user ID type
        var options = Helpers.TestDbContextFactory.CreateOptionsLong();
        var identityProvider = new TestIdentityProvider<long>();
        
        using var context = new TestDbContextLong(options, null!, identityProvider);
        var userId = 12345L;
        
        // Act
        var result = ((IDbContext<long>)context).WithUserId(userId);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IDbContext<long>>();
        result.Should().BeSameAs(context);
    }
    
    [Fact]
    public void WithUserId_Int_Should_Return_IDbContext_Interface()
    {
        // Test with int user ID type
        var options = Helpers.TestDbContextFactory.CreateOptionsInt();
        var identityProvider = new TestIdentityProvider<int>();
        
        using var context = new TestDbContextInt(options, null!, identityProvider);
        var userId = 42;
        
        // Act
        var result = ((IDbContext<int>)context).WithUserId(userId);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IDbContext<int>>();
        result.Should().BeSameAs(context);
    }
}
