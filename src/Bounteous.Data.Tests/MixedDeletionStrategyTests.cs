using AwesomeAssertions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bounteous.Data.Tests;

/// <summary>
/// Tests for mixed deletion strategies where parent and children have different deletion markers.
/// </summary>
public class MixedDeletionStrategyTests : IDisposable
{
    private readonly DbContextOptions<TestDbContext> dbContextOptions;
    private readonly IIdentityProvider<Guid> identityProvider;

    public MixedDeletionStrategyTests()
    {
        dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        identityProvider = new TestIdentityProvider<Guid>();
    }

    public void Dispose()
    {
        using var context = new TestDbContext(dbContextOptions, null, identityProvider);
        context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task SoftDelete_Parent_Should_PhysicallyDelete_HardDelete_Children()
    {
        // Arrange - Create a project (ISoftDelete) with files (IHardDelete)
        var userId = Guid.NewGuid();
        var project = new Project
        {
            Name = "Test Project",
            Files = new List<ProjectFile>
            {
                new ProjectFile { FileName = "file1.txt", FileSize = 1024 },
                new ProjectFile { FileName = "file2.txt", FileSize = 2048 },
                new ProjectFile { FileName = "file3.txt", FileSize = 4096 }
            }
        };

        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Projects.Add(project);
            await context.SaveChangesAsync();
        }

        var projectId = project.Id;
        var fileIds = project.Files.Select(f => f.Id).ToList();

        // Act - Soft delete the parent project
        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var existingProject = await context.Projects
                .Include(p => p.Files)
                .FirstAsync(p => p.Id == projectId);

            context.Projects.Remove(existingProject);
            await context.SaveChangesAsync();
        }

        // Assert - Parent should be soft deleted, children should be physically deleted
        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            // Parent project should be soft deleted (still in database with IsDeleted = true)
            var deletedProject = await context.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            Assert.NotNull(deletedProject);
            Assert.True(deletedProject.IsDeleted);

            // Children files should be physically deleted (not in database at all)
            var deletedFiles = await context.ProjectFiles
                .IgnoreQueryFilters()
                .Where(f => fileIds.Contains(f.Id))
                .ToListAsync();

            Assert.Empty(deletedFiles);
        }
    }

    [Fact]
    public async Task SoftDelete_Parent_Should_SoftDelete_SoftDelete_Children()
    {
        // Arrange - Create an order (ISoftDelete) with items (ISoftDelete)
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Description = "Test Order",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Product 1" },
                new OrderItem { ProductName = "Product 2" }
            }
        };

        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        var orderId = order.Id;
        var itemIds = order.Items.Select(i => i.Id).ToList();

        // Act - Soft delete the parent order
        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            context.WithUserIdTyped(userId);
            var existingOrder = await context.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.Id == orderId);

            context.Orders.Remove(existingOrder);
            await context.SaveChangesAsync();
        }

        // Assert - Both parent and children should be soft deleted
        await using (var context = new TestDbContext(dbContextOptions, null, identityProvider))
        {
            var deletedOrder = await context.Orders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            Assert.NotNull(deletedOrder);
            Assert.True(deletedOrder.IsDeleted);

            var deletedItems = await context.OrderItems
                .IgnoreQueryFilters()
                .Where(i => itemIds.Contains(i.Id))
                .ToListAsync();

            Assert.Equal(2, deletedItems.Count);
            Assert.All(deletedItems, item => Assert.True(item.IsDeleted));
        }
    }
}
