using AwesomeAssertions;
using Bounteous.Core.Time;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests;

public class GenericUserIdTests : IDisposable
{
    private readonly MockRepository mockRepository;
    private readonly Mock<IDbContextObserver> mockObserver;

    public GenericUserIdTests()
    {
        mockRepository = new MockRepository(MockBehavior.Strict);
        mockObserver = mockRepository.Create<IDbContextObserver>();
        
        mockObserver.Setup(x => x.OnSaved());
        mockObserver.Setup(x => x.Dispose());
        mockObserver.Setup(x => x.OnEntityTracked(It.IsAny<object>(), It.IsAny<Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs>()));
        mockObserver.Setup(x => x.OnStateChanged(It.IsAny<object>(), It.IsAny<Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs>()));
    }

    [Fact]
    public async Task DbContextBase_With_Long_UserId_Should_Set_Audit_Fields_On_Create()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var userId = 12345L;
        var product = new ProductWithLongUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            savedProduct.Should().NotBeNull();
            savedProduct!.CreatedBy.Should().Be(userId);
            savedProduct.ModifiedBy.Should().Be(userId);
            savedProduct.CreatedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.ModifiedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.Version.Should().Be(1);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Long_UserId_Should_Update_Audit_Fields_On_Modify()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var createUserId = 11111L;
        var updateUserId = 22222L;
        var product = new ProductWithLongUserId
        {
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.00m
        };

        // Act - Create
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(createUserId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        await Task.Delay(1000); // Ensure time difference

        // Act - Update
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(updateUserId);
            var existingProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            existingProduct.Name = "Updated Product";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            savedProduct.CreatedBy.Should().Be(createUserId);
            savedProduct.ModifiedBy.Should().Be(updateUserId);
            savedProduct.ModifiedOn.Should().BeAfter(savedProduct.CreatedOn);
            savedProduct.Version.Should().Be(2);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Long_UserId_Should_Handle_Null_UserId()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var product = new ProductWithLongUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act - Don't set user ID
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            savedProduct.CreatedBy.Should().BeNull();
            savedProduct.ModifiedBy.Should().BeNull();
            savedProduct.CreatedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.ModifiedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.Version.Should().Be(0);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Int_UserId_Should_Set_Audit_Fields_On_Create()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<int>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var userId = 42;
        var product = new ProductWithIntUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act
        await using (var context = new TestDbContextInt(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextInt(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            savedProduct.Should().NotBeNull();
            savedProduct!.CreatedBy.Should().Be(userId);
            savedProduct.ModifiedBy.Should().Be(userId);
            savedProduct.CreatedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.ModifiedOn.Should().BeCloseTo(Clock.Utc.Now, TimeSpan.FromSeconds(5));
            savedProduct.Version.Should().Be(1);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Int_UserId_Should_Update_Audit_Fields_On_Modify()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<int>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var createUserId = 100;
        var updateUserId = 200;
        var product = new ProductWithIntUserId
        {
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.00m
        };

        // Act - Create
        await using (var context = new TestDbContextInt(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(createUserId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        await Task.Delay(1000); // Ensure time difference

        // Act - Update
        await using (var context = new TestDbContextInt(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(updateUserId);
            var existingProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            existingProduct.Name = "Updated Product";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextInt(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            savedProduct.CreatedBy.Should().Be(createUserId);
            savedProduct.ModifiedBy.Should().Be(updateUserId);
            savedProduct.ModifiedOn.Should().BeAfter(savedProduct.CreatedOn);
            savedProduct.Version.Should().Be(2);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Long_UserId_Should_Support_Multiple_Entities()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var userId = 99999L;
        var product = new ProductWithLongUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };
        var customer = new CustomerWithLongUserId
        {
            Name = "Test Customer",
            Email = "test@example.com"
        };

        // Act
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            var savedCustomer = await context.Customers.FirstAsync(c => c.Id == customer.Id);
            
            savedProduct.CreatedBy.Should().Be(userId);
            savedProduct.ModifiedBy.Should().Be(userId);
            savedCustomer.CreatedBy.Should().Be(userId);
            savedCustomer.ModifiedBy.Should().Be(userId);
        }
    }

    [Fact]
    public async Task DbContextBase_With_Long_UserId_Should_Handle_Soft_Delete()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;

        var userId = 55555L;
        var product = new ProductWithLongUserId
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act - Create
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(userId);
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        // Act - Delete (soft delete)
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            context.WithUserIdTyped(userId);
            var existingProduct = await context.Products.FirstAsync(p => p.Id == product.Id);
            context.Products.Remove(existingProduct);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new TestDbContextLong(dbContextOptions, mockObserver.Object))
        {
            var savedProduct = await context.Products
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == product.Id);
            
            savedProduct.IsDeleted.Should().BeTrue();
            savedProduct.ModifiedBy.Should().Be(userId);
        }
    }

    public void Dispose()
        => mockRepository.VerifyAll();
}
