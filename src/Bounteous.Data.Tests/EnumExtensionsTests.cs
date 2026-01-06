using System.ComponentModel;
using AwesomeAssertions;
using Bounteous.Data.Extensions;

namespace Bounteous.Data.Tests;

public class EnumExtensionsTests
{
    [Fact]
    public void GetDescription_Should_Return_Description_Attribute_Value()
    {
        // Arrange
        var status = TestStatus.Active;

        // Act
        var description = status.GetDescription();

        // Assert
        description.Should().Be("Currently Active");
    }

    [Fact]
    public void GetDescription_Should_Return_Enum_Name_When_No_Description()
    {
        // Arrange
        var status = TestStatus.Unknown;

        // Act
        var description = status.GetDescription();

        // Assert
        description.Should().Be("Unknown");
    }

    [Fact]
    public void GetDescription_Should_Throw_For_Non_Enum_Type()
    {
        // Arrange
        var value = 42;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => value.GetDescription());
    }

    [Fact]
    public void FromDescription_Should_Return_Correct_Enum_Value()
    {
        // Arrange
        var description = "Currently Active";

        // Act
        var status = description.FromDescription<TestStatus>();

        // Assert
        status.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void FromDescription_Should_Return_Enum_By_Name_When_No_Description()
    {
        // Arrange
        var description = "Unknown";

        // Act
        var status = description.FromDescription<TestStatus>();

        // Assert
        status.Should().Be(TestStatus.Unknown);
    }

    [Fact]
    public void FromDescription_Should_Throw_For_Null_Or_Empty_String()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => string.Empty.FromDescription<TestStatus>());
        Assert.Throws<ArgumentNullException>(() => ((string)null!).FromDescription<TestStatus>());
    }

    [Fact]
    public void FromDescription_Should_Handle_Multiple_Enum_Values()
    {
        // Arrange & Act
        var pending = "Waiting".FromDescription<TestStatus>();
        var completed = "Finished".FromDescription<TestStatus>();
        var cancelled = "Stopped".FromDescription<TestStatus>();

        // Assert
        pending.Should().Be(TestStatus.Pending);
        completed.Should().Be(TestStatus.Completed);
        cancelled.Should().Be(TestStatus.Cancelled);
    }

    [Fact]
    public void GetDescription_And_FromDescription_Should_Be_Reversible()
    {
        // Arrange
        var originalStatus = TestStatus.Active;

        // Act
        var description = originalStatus.GetDescription();
        var roundTrip = description.FromDescription<TestStatus>();

        // Assert
        roundTrip.Should().Be(originalStatus);
    }
}

// Test enum for EnumExtensions tests
public enum TestStatus
{
    Unknown,
    
    [Description("Currently Active")]
    Active,
    
    [Description("Waiting")]
    Pending,
    
    [Description("Finished")]
    Completed,
    
    [Description("Stopped")]
    Cancelled
}
