using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests;

public class TestDbContextFactoryTests : IDisposable
{
    private readonly MockRepository mockRepository;
    private readonly TestDbContextFactory factory;

    public TestDbContextFactoryTests()
    {
        mockRepository = new MockRepository(MockBehavior.Strict);
        var connectionBuilderMock = mockRepository.Create<IConnectionBuilder>();
        var observerMock = mockRepository.Create<IDbContextObserver>();
        factory = new TestDbContextFactory(connectionBuilderMock.Object, observerMock.Object);
    }

    [Fact]
    public void CreateShouldReturnTestDbContext()
    {
        // Act
        var context = factory.Create();

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<TestDbContext>();
    }

    [Fact]
    public void CreateWithOptionsAndObserverShouldReturnTestDbContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        // Act
        var context = factory.Create();

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<TestDbContext>();
    }

    public void Dispose() => mockRepository.VerifyAll();
}