using Bounteous.DotNet.Data.Tests.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.DotNet.Data.Tests;

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
    public void Create_ShouldReturnTestDbContext()
    {
        // Act
        var context = factory.Create();

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<TestDbContext>();
    }

    [Fact]
    public void Create_WithOptionsAndObserver_ShouldReturnTestDbContext()
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

    public void Dispose()
        => mockRepository.VerifyAll();
}