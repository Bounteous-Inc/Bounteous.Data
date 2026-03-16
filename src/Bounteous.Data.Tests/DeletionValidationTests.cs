using AwesomeAssertions;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Tests.Context;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests;

public class DeletionValidationTests
{
    [Fact]
    public void DbContext_Should_Throw_When_Entity_Implements_Both_ISoftDelete_And_IHardDelete()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<InvalidTestContext>()
            .UseInMemoryDatabase(databaseName: $"ValidationTestDb_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var context = new InvalidTestContext(options);
            _ = context.Model; // Trigger model creation
        });

        exception.Message.Should().Contain("cannot implement both ISoftDelete and IHardDelete");
    }

    [Fact]
    public void DbContext_Should_Throw_When_AuditBase_Entity_Has_No_Deletion_Strategy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MissingStrategyTestContext>()
            .UseInMemoryDatabase(databaseName: $"ValidationTestDb_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var context = new MissingStrategyTestContext(options);
            _ = context.Model; // Trigger model creation
        });

        exception.Message.Should().Contain("does not implement ISoftDelete or IHardDelete");
        exception.Message.Should().Contain("must explicitly choose a deletion strategy");
    }
}

// Test entities for validation
public class InvalidEntity : AuditBase<Guid, Guid>, ISoftDelete, IHardDelete
{
    public bool IsDeleted { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class MissingStrategyEntity : AuditBase<Guid, Guid>
{
    public string Name { get; set; } = string.Empty;
}

// Test contexts
public class InvalidTestContext : DbContextBase<Guid>
{
    public InvalidTestContext(DbContextOptions options) 
        : base(options, null, new TestIdentityProvider<Guid>())
    {
    }

    public DbSet<InvalidEntity> InvalidEntities { get; set; } = null!;

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvalidEntity>();
    }
}

public class MissingStrategyTestContext : DbContextBase<Guid>
{
    public MissingStrategyTestContext(DbContextOptions options) 
        : base(options, null, new TestIdentityProvider<Guid>())
    {
    }

    public DbSet<MissingStrategyEntity> MissingStrategyEntities { get; set; } = null!;

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MissingStrategyEntity>();
    }
}
