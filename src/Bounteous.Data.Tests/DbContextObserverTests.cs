using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

namespace Bounteous.Data.Tests;

public class DbContextObserverTests : IDisposable
{
    private readonly MockRepository mockRepository;
    private readonly Mock<IDbContextObserver> mockObserver;
    private readonly DbContextOptions dbContextOptions;

    public DbContextObserverTests()
    {
        mockRepository = new MockRepository(MockBehavior.Strict);
        mockObserver = mockRepository.Create<IDbContextObserver>();
        dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        mockObserver.Setup(x => x.OnSaved());
        mockObserver.Setup(x => x.Dispose());
    }

    [Fact]
    public async Task Observer_Should_Be_Wired_Up_Correctly()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };

        mockObserver
            .Setup(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()))
            .Callback<object, EntityTrackedEventArgs>((_, e) =>
            {
                var tracked = e.Entry.Entity as Customer;
                customer.Should().BeSameAs(tracked);
            });
        
        mockObserver
            .Setup(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()));
        
        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        mockObserver.Verify(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()),
            Times.Once);  
        mockObserver.Verify(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()),
            Times.Once);
        
        mockObserver.Verify(x => x.OnSaved(), Times.Once);
        mockObserver.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task Observer_Should_Receive_Entities_In_Order_They_Were_Added()
    {
        // Arrange

        var trackedEntities = new List<object>();

        mockObserver
            .Setup(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()))
            .Callback<object, EntityTrackedEventArgs>((_, e) => { trackedEntities.Add(e.Entry.Entity); });

        //OnStateChanged
        mockObserver
            .Setup(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()));
        mockObserver
            .Setup(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()));
        mockObserver
            .Setup(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()));

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            var customer = new Customer { Name = "Customer1" };
            var order = new Order { CustomerId = customer.Id, Description = "Order1" };
            var orderItem = new OrderItem { OrderId = order.Id, ProductName = "Product1" };

            context.Customers.Add(customer);
            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);

            await context.SaveChangesAsync();
        }

        // Assert
        mockObserver.Verify(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()),
            Times.Exactly(3));
        
        mockObserver.Verify(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()),
            Times.Exactly(3));
        
        mockObserver.Verify(x => x.OnSaved(), Times.Once);
        mockObserver.Verify(x => x.Dispose(), Times.Once);
        
        trackedEntities.Count.Should().Be(3);
        trackedEntities[0].Should().BeOfType<Customer>();
        trackedEntities[1].Should().BeOfType<Order>();
        trackedEntities[2].Should().BeOfType<OrderItem>();
    }

    [Fact]
    public async Task Observer_Should_Be_Notified_On_State_Changed()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer" };

        mockObserver
            .Setup(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()));

        mockObserver
            .Setup(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()))
            .Callback<object, EntityStateChangedEventArgs>((_, e) =>
            {
                e.Entry.Entity.Should().BeSameAs(customer);
                e.OldState.Should().Be(EntityState.Added); //this is the OldState since this if fired AFTER the Save occurs in case it fails.
            });

        // Act
        await using (var context = new TestDbContext(dbContextOptions, mockObserver.Object))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Assert
        
        mockObserver.Verify(o => o.OnEntityTracked(It.IsAny<object>(), It.IsAny<EntityTrackedEventArgs>()),
            Times.Once);
        mockObserver.Verify(o => o.OnStateChanged(It.IsAny<object>(), It.IsAny<EntityStateChangedEventArgs>()),
            Times.Once);
        
        mockObserver.Verify(x => x.OnSaved(), Times.Once);
        mockObserver.Verify(x => x.Dispose(), Times.Once);
    }

    public void Dispose()
        => mockRepository.VerifyAll();
}