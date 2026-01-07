using AwesomeAssertions;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests;

public class QueryableExtensionsTests
{
    private readonly DbContextOptions<DbContextBase> dbContextOptions;
    private readonly Mock<IDbContextObserver> mockObserver;

    public QueryableExtensionsTests()
    {
        dbContextOptions = new DbContextOptionsBuilder<DbContextBase>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;
        
        mockObserver = new Mock<IDbContextObserver>(MockBehavior.Loose);
    }

    [Fact]
    public async Task WhereIf_Should_Apply_Predicate_When_Condition_True()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.AddRange(
                new Customer { Name = "Alice" },
                new Customer { Name = "Bob" },
                new Customer { Name = "Charlie" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers
                .AsQueryable()
                .WhereIf(true, c => c.Name.StartsWith("A"))
                .ToListAsync();
            
            // Assert
            results.Count.Should().Be(1);
            results[0].Name.Should().Be("Alice");
        }
    }

    [Fact]
    public async Task WhereIf_Should_Not_Apply_Predicate_When_Condition_False()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.AddRange(
                new Customer { Name = "Alice" },
                new Customer { Name = "Bob" },
                new Customer { Name = "Charlie" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers
                .AsQueryable()
                .WhereIf(false, c => c.Name.StartsWith("A"))
                .ToListAsync();
            
            // Assert
            results.Count.Should().Be(3);
        }
    }

    [Fact]
    public async Task WhereIf_Should_Chain_Multiple_Conditions()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.AddRange(
                new Customer { Name = "Alice" },
                new Customer { Name = "Amanda" },
                new Customer { Name = "Bob" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers
                .AsQueryable()
                .WhereIf(true, c => c.Name.StartsWith("A"))
                .WhereIf(true, c => c.Name.Contains("man"))
                .ToListAsync();
            
            // Assert
            results.Count.Should().Be(1);
            results[0].Name.Should().Be("Amanda");
        }
    }

    [Fact]
    public async Task ToPaginatedListAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            for (int i = 1; i <= 10; i++)
            {
                context.Customers.Add(new Customer { Name = $"Customer {i:D2}" });
            }
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var page1 = await context.Customers
                .OrderBy(c => c.Name)
                .ToPaginatedListAsync(1, 3);
            
            var page2 = await context.Customers
                .OrderBy(c => c.Name)
                .ToPaginatedListAsync(2, 3);
            
            // Assert
            page1.Count.Should().Be(3);
            page1[0].Name.Should().Be("Customer 01");
            
            page2.Count.Should().Be(3);
            page2[0].Name.Should().Be("Customer 04");
        }
    }

    [Fact]
    public async Task ToPaginatedListAsync_Should_Handle_Empty_Results()
    {
        // Arrange & Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers
                .Where(c => c.Name == "NonExistent")
                .ToPaginatedListAsync(1, 10);
            
            // Assert
            results.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ToPaginatedListAsync_Should_Handle_Page_Beyond_Results()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(new Customer { Name = "Customer 1" });
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers
                .ToPaginatedListAsync(5, 10);
            
            // Assert
            results.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ToPaginatedEnumerableAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            for (int i = 1; i <= 10; i++)
            {
                context.Customers.Add(new Customer { Name = $"Customer {i:D2}" });
            }
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var page = context.Customers
                .OrderBy(c => c.Name)
                .ToPaginatedEnumerableAsync(2, 3);
            
            var results = new List<Customer>();
            await foreach (var item in page)
            {
                results.Add(item);
            }
            
            // Assert
            results.Count.Should().Be(3);
            results[0].Name.Should().Be("Customer 04");
        }
    }

    [Fact]
    public async Task ToPaginatedListAsync_Should_Use_Default_Page_Size()
    {
        // Arrange
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            for (int i = 1; i <= 60; i++)
            {
                context.Customers.Add(new Customer { Name = $"Customer {i}" });
            }
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var results = await context.Customers.ToPaginatedListAsync(); // Default page 1, size 50
            
            // Assert
            results.Count.Should().Be(50);
        }
    }

    [Fact]
    public async Task FindById_Guid_Should_Find_Existing_Customer()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var found = await context.Customers.FindById(customer.Id);
            
            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be(customer.Id);
            found.Name.Should().Be("Test Customer");
        }
    }

    [Fact]
    public async Task FindById_Guid_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            await Assert.ThrowsAsync<NotFoundException<Customer>>(
                async () => await context.Customers.FindById(nonExistentId));
        }
    }

    [Fact]
    public async Task FindById_Guid_Should_Work_With_Multiple_Entities()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1" };
        var customer2 = new Customer { Name = "Customer 2" };
        var customer3 = new Customer { Name = "Customer 3" };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.AddRange(customer1, customer2, customer3);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var found = await context.Customers.FindById(customer2.Id);
            
            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be(customer2.Id);
            found.Name.Should().Be("Customer 2");
        }
    }

    [Fact]
    public async Task FindById_Guid_Should_Work_Without_Includes_Parameter()
    {
        // Arrange
        var order = new Order 
        { 
            CustomerId = Guid.NewGuid(),
            Description = "Test Order"
        };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var found = await context.Orders.FindById(order.Id);
            
            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be(order.Id);
            found.Description.Should().Be("Test Order");
        }
    }

    [Fact]
    public async Task IncludeIf_Should_Include_Navigation_Property_When_Condition_True()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };
        var order = new Order 
        { 
            CustomerId = customer.Id,
            Description = "Test Order"
        };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var query = context.Orders
                .AsQueryable()
                .IncludeIf(true, o => o.Customer);
            
            var result = await query.FirstOrDefaultAsync();
            
            // Assert
            result.Should().NotBeNull();
            result!.Customer.Should().NotBeNull();
            result.Customer.Name.Should().Be("Test Customer");
        }
    }

    [Fact]
    public async Task IncludeIf_Should_Not_Include_Navigation_Property_When_Condition_False()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };
        var order = new Order 
        { 
            CustomerId = customer.Id,
            Description = "Test Order"
        };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var query = context.Orders
                .AsQueryable()
                .IncludeIf(false, o => o.Customer);
            
            var result = await query.FirstOrDefaultAsync();
            
            // Assert
            result.Should().NotBeNull();
            result!.Customer.Should().BeNull();
        }
    }

    [Fact]
    public async Task IncludeIf_Should_Chain_Multiple_Includes()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };
        var order = new Order 
        { 
            CustomerId = customer.Id,
            Description = "Test Order"
        };
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductName = "Test Product"
        };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var query = context.Orders
                .AsQueryable()
                .IncludeIf(true, o => o.Customer)
                .IncludeIf(true, o => o.Items);
            
            var result = await query.FirstOrDefaultAsync();
            
            // Assert
            result.Should().NotBeNull();
            result!.Customer.Should().NotBeNull();
            result.Customer.Name.Should().Be("Test Customer");
            result.Items.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductName.Should().Be("Test Product");
        }
    }

    [Fact]
    public async Task IncludeIf_Should_Conditionally_Include_Based_On_Multiple_Flags()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };
        var order = new Order 
        { 
            CustomerId = customer.Id,
            Description = "Test Order"
        };
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductName = "Test Product"
        };
        
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();
        }

        // Act - Include customer but not items
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var query = context.Orders
                .AsQueryable()
                .IncludeIf(true, o => o.Customer)
                .IncludeIf(false, o => o.Items);
            
            var result = await query.FirstOrDefaultAsync();
            
            // Assert
            result.Should().NotBeNull();
            result!.Customer.Should().NotBeNull();
            result.Customer.Name.Should().Be("Test Customer");
            result.Items.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task IncludeIf_Should_Work_With_Empty_QuerySet()
    {
        // Arrange & Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var query = context.Orders
                .AsQueryable()
                .Where(o => o.Description == "NonExistent")
                .IncludeIf(true, o => o.Customer);
            
            var result = await query.FirstOrDefaultAsync();
            
            // Assert
            result.Should().BeNull();
        }
    }
}
