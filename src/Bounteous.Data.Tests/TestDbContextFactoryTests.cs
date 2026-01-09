using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests;

public class TestDbContextFactoryTests : IDisposable
{
    private readonly MockRepository mockRepository;
    private readonly TestDbContextFactory factory;
    private readonly Mock<IConnectionBuilder> mockConnectionBuilder;
    private readonly Mock<IDbContextObserver> mockObserver;

    public TestDbContextFactoryTests()
    {
        mockRepository = new MockRepository(MockBehavior.Strict);
        mockConnectionBuilder = mockRepository.Create<IConnectionBuilder>();
        mockObserver = mockRepository.Create<IDbContextObserver>();
        var identityProvider = new TestIdentityProvider<Guid>();
        factory = new TestDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object, identityProvider);
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