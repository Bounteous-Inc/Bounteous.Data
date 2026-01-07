using AwesomeAssertions;
using Bounteous.Data.Extensions;

namespace Bounteous.Data.Tests;

public class GuidExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_Should_Return_True_For_Empty_Guid()
    {
        // Arrange
        Guid? guid = Guid.Empty;

        // Act
        var result = guid.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_Should_Return_True_For_Null_Guid()
    {
        // Arrange
        Guid? guid = null;

        // Act
        var result = guid.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_Should_Return_False_For_Valid_Guid()
    {
        // Arrange
        Guid? guid = Guid.NewGuid();

        // Act
        var result = guid.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNullOrEmpty_Should_Return_False_For_Non_Null_Nullable_Guid()
    {
        // Arrange
        Guid? guid = Guid.NewGuid();

        // Act
        var result = guid.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNullOrEmpty_Should_Handle_Multiple_Guids()
    {
        // Arrange
        Guid? emptyGuid = Guid.Empty;
        Guid? validGuid = Guid.NewGuid();
        Guid? nullGuid = null;
        Guid? validNullableGuid = Guid.NewGuid();

        // Act & Assert
        emptyGuid.IsNullOrEmpty().Should().BeTrue();
        validGuid.IsNullOrEmpty().Should().BeFalse();
        nullGuid.IsNullOrEmpty().Should().BeTrue();
        validNullableGuid.IsNullOrEmpty().Should().BeFalse();
    }
}
