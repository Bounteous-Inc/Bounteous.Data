using AwesomeAssertions;
using Bounteous.Data.Domain.ReadOnly;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Tests.Domain;

/// <summary>
/// Tests for ReadOnlyDbSet fail-fast validation.
/// Verifies that write operations throw immediately rather than deferring to SaveChanges.
/// </summary>
public class ReadOnlyDbSetTests : DbContextTestBase
{
    /// <summary>
    /// Creates a test product with unique ID for testing.
    /// </summary>
    private static ReadOnlyLegacyProduct CreateTestProduct(long id, string name = "Test Product") =>
        new()
        {
            Id = id,
            Name = name,
            Price = 50.00m,
            Category = "Test"
        };

    /// <summary>
    /// Creates multiple test products for range operations.
    /// </summary>
    private static ReadOnlyLegacyProduct[] CreateTestProducts(long startId, int count) =>
        Enumerable.Range(0, count)
            .Select(i => CreateTestProduct(startId + i, $"Product {i + 1}"))
            .ToArray();

    /// <summary>
    /// Creates a test customer with unique ID for testing.
    /// </summary>
    private static ReadOnlyLegacyCustomer CreateTestCustomer(int id) =>
        new()
        {
            Id = id,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedDate = DateTime.UtcNow
        };

    /// <summary>
    /// Helper to verify ReadOnlyEntityException is thrown with expected message content.
    /// </summary>
    private static void AssertReadOnlyException(ReadOnlyEntityException exception, string entityName, string operation)
    {
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(operation);
    }
    [Fact]
    public async Task ReadOnlyDbSet_Should_Allow_Query()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            // ReadOnlyDbSet implements IQueryable - safe async operations work directly
            var count = await readOnlySet.CountAsync();
            
            count.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Allow_LINQ_Queries()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            // ReadOnlyDbSet implements IQueryable - full LINQ support works directly
            var count = await readOnlySet
                .Where(p => p.Category == "Electronics")
                .Where(p => p.Price > 100m)
                .CountAsync();
            
            count.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_Add()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var product = CreateTestProduct(1000L);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.Add(product));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyProduct", "create");
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Throw_Immediately_On_AddAsync()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var product = CreateTestProduct(1001L, "Test Product Async");

            var exception = await Assert.ThrowsAsync<ReadOnlyEntityException>(async () => 
                await readOnlySet.AddAsync(product));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyProduct", "create");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_AddRange()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var products = CreateTestProducts(1002L, 2);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.AddRange(products));
            
            exception.Message.Should().Contain("create");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_Remove()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var product = CreateTestProduct(2000L, "To Delete");

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.Remove(product));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyProduct", "delete");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_RemoveRange()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var products = CreateTestProducts(2001L, 2);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.RemoveRange(products));
            
            exception.Message.Should().Contain("delete");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_Update()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var product = CreateTestProduct(3000L, "To Update");

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.Update(product));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyProduct", "update");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_UpdateRange()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var products = CreateTestProducts(3001L, 2);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.UpdateRange(products));
            
            exception.Message.Should().Contain("update");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_Attach()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var product = CreateTestProduct(4000L, "To Attach");

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.Attach(product));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyProduct", "attach");
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Throw_Immediately_On_AttachRange()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            var products = CreateTestProducts(4001L, 2);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.AttachRange(products));
            
            exception.Message.Should().Contain("attach");
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_With_Int_Id_Should_Throw_On_Add()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyCustomer>().AsReadOnly<ReadOnlyLegacyCustomer, int>();
            var customer = CreateTestCustomer(100);

            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlySet.Add(customer));
            
            AssertReadOnlyException(exception, "ReadOnlyLegacyCustomer", "create");
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Support_Find()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            var product = ((DbSet<ReadOnlyLegacyProduct>)readOnlySet).Find(999L);
            
            product.Should().BeNull();
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Support_FindAsync()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            var product = await ((DbSet<ReadOnlyLegacyProduct>)readOnlySet).FindAsync(999L);
            
            product.Should().BeNull();
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Support_AsQueryable()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            var queryable = ((DbSet<ReadOnlyLegacyProduct>)readOnlySet).AsQueryable();
            
            queryable.Should().NotBeNull();
            var result = await queryable.ToListAsync();
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ReadOnlyDbSet_Should_Support_AsAsyncEnumerable()
    {
        await using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            var asyncEnumerable = ((DbSet<ReadOnlyLegacyProduct>)readOnlySet).AsAsyncEnumerable();
            
            asyncEnumerable.Should().NotBeNull();
            
            var count = 0;
            await foreach (var item in asyncEnumerable)
            {
                count++;
            }
            
            count.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Should_Implement_IQueryable()
    {
        using (var context = CreateContext())
        {
            var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            // ReadOnlyDbSet implements IQueryable directly
            IQueryable<ReadOnlyLegacyProduct> queryable = readOnlySet;
            queryable.Should().NotBeNull();
            queryable.ElementType.Should().Be(typeof(ReadOnlyLegacyProduct));
            queryable.Expression.Should().NotBeNull();
            queryable.Provider.Should().NotBeNull();
        }
    }

    [Fact]
    public void ReadOnlyDbSet_Comparison_With_Regular_DbSet()
    {
        using (var context = CreateContext())
        {
            var regularDbSet = context.Set<LegacyProduct>();
            var readOnlyDbSet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly<ReadOnlyLegacyProduct, long>();
            
            var writableProduct = new LegacyProduct { Id = 5000L, Name = "Writable", Price = 10m };
            var readonlyProduct = CreateTestProduct(5001L, "Readonly");
            
            // Regular DbSet allows Add (exception happens at SaveChanges)
            regularDbSet.Add(writableProduct);
            
            // ReadOnlyDbSet throws immediately
            var exception = Assert.Throws<ReadOnlyEntityException>(() => readOnlyDbSet.Add(readonlyProduct));
            
            exception.Should().NotBeNull();
        }
    }
}
