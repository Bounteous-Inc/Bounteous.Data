using AwesomeAssertions;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Tests;

/// <summary>
/// DbContext exposes four SaveChanges entry points. EF Core routes the parameterless ones
/// through SaveChanges(bool) and SaveChangesAsync(bool, CancellationToken), so every
/// overload must get the same read-only checks, auditing and soft delete handling.
/// </summary>
public class SaveChangesOverloadTests : DbContextTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveChangesAsync_With_AcceptAllChanges_Is_Blocked_By_ReadOnlyRequestScope(bool acceptAllChangesOnSuccess)
    {
        await using var context = CreateContext();

        using (new ReadOnlyRequestScope())
        {
            context.Customers.Add(new Customer { Name = "Test Customer" });

            var act = async () => await context.SaveChangesAsync(acceptAllChangesOnSuccess);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*read-only request scope*");
        }

        await using var verifyContext = CreateContext();
        (await verifyContext.Customers.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChanges_Throws_For_ReadOnly_Entity()
    {
        await using var context = CreateContext();
        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
        {
            Id = 9000L,
            Name = "New Product",
            Price = 50.00m,
            Category = "Books"
        });

        var act = async () => await context.SaveChangesAsync(acceptAllChangesOnSuccess: true);

        await act.Should().ThrowAsync<ReadOnlyEntityException>();
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChanges_Sets_Audit_Fields_On_Create()
    {
        var userId = Guid.NewGuid();
        var customer = new Customer { Name = "Audited Customer" };

        await using (var context = CreateContextWithUserId(userId))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync(acceptAllChangesOnSuccess: true);
        }

        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Customers.SingleAsync(c => c.Id == customer.Id);
        saved.CreatedBy.Should().Be(userId);
        saved.ModifiedBy.Should().Be(userId);
        saved.CreatedOn.Should().NotBe(default);
        saved.Version.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChanges_False_Audits_And_Leaves_Changes_Pending()
    {
        // The pattern EF Core recommends for retriable transactions with an execution strategy
        var userId = Guid.NewGuid();
        await using var context = CreateContextWithUserId(userId);
        var customer = new Customer { Name = "Deferred Accept" };
        context.Customers.Add(customer);

        await context.SaveChangesAsync(acceptAllChangesOnSuccess: false);

        context.Entry(customer).State.Should().Be(EntityState.Added);
        customer.CreatedBy.Should().Be(userId);

        context.ChangeTracker.AcceptAllChanges();
        context.Entry(customer).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChanges_Soft_Deletes_ISoftDelete_Entity()
    {
        var customer = new Customer { Name = "To Soft Delete" };
        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.Add(customer);
            await setupContext.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var loaded = await context.Customers.SingleAsync(c => c.Id == customer.Id);
            context.Customers.Remove(loaded);
            await context.SaveChangesAsync(acceptAllChangesOnSuccess: true);
        }

        await using var verifyContext = CreateContext();
        var row = await verifyContext.Customers.IgnoreQueryFilters().SingleOrDefaultAsync(c => c.Id == customer.Id);
        row.Should().NotBeNull();
        row!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChanges_Notifies_Observer()
    {
        await using var context = CreateContext();
        context.Customers.Add(new Customer { Name = "Observed" });

        await context.SaveChangesAsync(acceptAllChangesOnSuccess: true);

        MockObserver.Verify(x => x.OnSaved(), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_Without_Arguments_Audits_Exactly_Once()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateContextWithUserId(userId);
        var customer = new Customer { Name = "Single Pass" };
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        customer.Version.Should().Be(1);
        MockObserver.Verify(x => x.OnSaved(), Times.Once);
    }
}
