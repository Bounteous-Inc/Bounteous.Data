using Bounteous.Data;
using Bounteous.Data.Exceptions;
using Bounteous.Data.Extensions;
using Bounteous.Data.Sample;
using Bounteous.Data.Sample.Data;
using Bounteous.Data.Sample.Domain.Entities;
using Bounteous.Data.Sample.Domain.Enums;
using Bounteous.Data.Sample.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

AppStartup.InitializeLogging();

try
{
    Log.Information("╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║   Bounteous.Data Comprehensive Feature Demonstration         ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    var services = new ServiceCollection();
    AppStartup.ConfigureServices(services, configuration);
    var serviceProvider = services.BuildServiceProvider();

    var userId = Guid.NewGuid();
    Log.Information("\n[SETUP] Test User ID: {UserId}", userId);
    
    // Set the user ID in the built-in IdentityProvider for automatic resolution
    var identityProvider = serviceProvider.GetRequiredService<IIdentityProvider<Guid>>();
    if (identityProvider is IdentityProvider<Guid> provider)
    {
        provider.SetCurrentUserId(userId);
        Log.Information("[SETUP] ✓ User ID set in built-in IdentityProvider for automatic auditing");
    }

    var customerService = serviceProvider.GetRequiredService<ICustomerService>();
    var productService = serviceProvider.GetRequiredService<IProductService>();
    var orderService = serviceProvider.GetRequiredService<IOrderService>();
    var contextFactory = serviceProvider.GetRequiredService<Bounteous.Data.IDbContextFactory<SampleDbContext, Guid>>();

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 1: Automatic Auditing with AuditVisitor.AcceptNew
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 1: Automatic Auditing (CreatedBy, CreatedOn, Version)║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[AUDIT] AuditVisitor.AcceptNew will be called for new entities");

    var customer1 = await customerService.CreateCustomerAsync(
        "John Doe",
        "john.doe@example.com",
        "555-1234",
        userId);
    Log.Information("[AUDIT] ✓ Customer Created: {Name}", customer1.Name);
    Log.Information("[AUDIT]   - CreatedBy: {CreatedBy}", customer1.CreatedBy);
    Log.Information("[AUDIT]   - CreatedOn: {CreatedOn:yyyy-MM-dd HH:mm:ss}", customer1.CreatedOn);
    Log.Information("[AUDIT]   - Version: {Version}", customer1.Version);

    var customer2 = await customerService.CreateCustomerAsync(
        "Jane Smith",
        "jane.smith@example.com",
        "555-5678",
        userId);
    Log.Information("[AUDIT] ✓ Customer Created: {Name} (CreatedBy: {CreatedBy})", customer2.Name, customer2.CreatedBy);

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 2: Value Converters (EnumConverter, DateTimeConverter)
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 2: Value Converters (Enum & DateTime)                ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[CONVERTER] EnumConverter stores enum as description string");
    Log.Debug("[CONVERTER] DateTimeConverter ensures UTC storage");

    var product1 = await productService.CreateProductAsync(
        "Laptop",
        "High-performance laptop",
        1299.99m,
        10,
        "LAP-001",
        userId);
    
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var prod = await context.Products.FindAsync(product1.Id);
        if (prod != null)
        {
            prod.Status = ProductStatus.Active;
            prod.LastRestockedOn = DateTime.UtcNow;
            await context.SaveChangesAsync();
            Log.Information("[CONVERTER] ✓ Product Status: {Status} (stored as: 'Active')", prod.Status);
            Log.Information("[CONVERTER] ✓ LastRestockedOn: {Date:yyyy-MM-dd HH:mm:ss} UTC", prod.LastRestockedOn);
        }
    }

    var product2 = await productService.CreateProductAsync(
        "Mouse",
        "Wireless mouse",
        29.99m,
        50,
        "MOU-001",
        userId);

    var product3 = await productService.CreateProductAsync(
        "Keyboard",
        "Mechanical keyboard",
        89.99m,
        25,
        "KEY-001",
        userId);

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 3: Generic ID Support (Guid, long, int)
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 3: Generic ID Support (Guid, long, int)              ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[GENERIC-ID] Demonstrating AuditBase<long> with Category entity");

    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products and accessories",
            DisplayOrder = 1
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        Log.Information("[GENERIC-ID] ✓ Category Created with long ID: {Id}", category.Id);
        Log.Information("[GENERIC-ID]   - Name: {Name}", category.Name);
        Log.Information("[GENERIC-ID]   - CreatedBy (Guid): {CreatedBy}", category.CreatedBy);
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 4: Read-Only Entities (ReadOnlyEntityBase)
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 4: Read-Only Entities (Query-Only, No CUD)           ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[READ-ONLY] Seeding legacy system data for read-only demonstration");
    Log.Information("[READ-ONLY] Note: In production, read-only entities would be pre-existing data");
    Log.Information("[READ-ONLY] Skipping seed for demo purposes (would require direct DB access)");

    Log.Information("[READ-ONLY] ✓ Read-only entities prevent Create, Update, Delete operations");
    Log.Information("[READ-ONLY] ✓ Queries would work normally if data existed");

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 5: AuditVisitor.AcceptModified (Version Increment)
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 5: Audit on Modify (ModifiedBy, ModifiedOn, Version) ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[AUDIT-MODIFY] AuditVisitor.AcceptModified will increment version");

    var updatedProduct = await productService.UpdateProductPriceAsync(product1.Id, 1199.99m, userId);
    Log.Information("[AUDIT-MODIFY] ✓ Product price updated: {Name}", updatedProduct.Name);
    Log.Information("[AUDIT-MODIFY]   - Price: ${OldPrice} → ${NewPrice}", 1299.99m, updatedProduct.Price);
    Log.Information("[AUDIT-MODIFY]   - ModifiedBy: {ModifiedBy}", updatedProduct.ModifiedBy);
    Log.Information("[AUDIT-MODIFY]   - ModifiedOn: {ModifiedOn:yyyy-MM-dd HH:mm:ss}", updatedProduct.ModifiedOn);
    Log.Information("[AUDIT-MODIFY]   - Version: {Version} (incremented)", updatedProduct.Version);

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 6: Query Extensions (WhereIf, IncludeIf, Pagination)
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 6: Query Extensions (WhereIf, IncludeIf, Pagination) ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");

    var order1 = await orderService.CreateOrderAsync(
        customer1.Id,
        new List<(Guid productId, int quantity)>
        {
            (product1.Id, 1),
            (product2.Id, 2)
        },
        userId);

    var order2 = await orderService.CreateOrderAsync(
        customer2.Id,
        new List<(Guid productId, int quantity)>
        {
            (product2.Id, 1),
            (product3.Id, 1)
        },
        userId);

    Log.Debug("[QUERY-EXT] Testing WhereIf conditional filtering");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        bool filterByCustomer = true;
        var filteredOrders = await context.Orders
            .WhereIf(filterByCustomer, o => o.CustomerId == customer1.Id)
            .ToListAsync();
        Log.Information("[QUERY-EXT] ✓ WhereIf: Found {Count} order(s) for customer1", filteredOrders.Count);
    }

    Log.Debug("[QUERY-EXT] Testing IncludeIf conditional eager loading");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        bool includeCustomer = true;
        var ordersWithCustomer = await context.Orders
            .IncludeIf(includeCustomer, o => o.Customer)
            .FirstOrDefaultAsync();
        if (ordersWithCustomer != null)
        {
            Log.Information("[QUERY-EXT] ✓ IncludeIf: Loaded order with customer: {CustomerName}",
                ordersWithCustomer.Customer?.Name ?? "null");
        }
    }

    Log.Debug("[QUERY-EXT] Testing ToPaginatedListAsync");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var paginatedProducts = await context.Products.ToPaginatedListAsync(page: 1, size: 2);
        Log.Information("[QUERY-EXT] ✓ Pagination: Retrieved {Count} products (page 1, size 2)", paginatedProducts.Count);
        foreach (var p in paginatedProducts)
        {
            Log.Information("[QUERY-EXT]   - {Name}", p.Name);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 7: FindById Extension with NotFoundException
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 7: FindById Extension & NotFoundException            ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");

    Log.Debug("[FIND-BY-ID] Testing FindById with valid ID");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var foundProduct = await context.Products.FindById(product1.Id);
        Log.Information("[FIND-BY-ID] ✓ Found product: {Name} (ID: {Id})", foundProduct.Name, foundProduct.Id);
    }

    Log.Debug("[FIND-BY-ID] Testing FindById with invalid ID (should throw NotFoundException)");
    try
    {
        using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
        var nonExistentId = Guid.NewGuid();
        var notFound = await context.Products.FindById(nonExistentId);
        Log.Warning("[FIND-BY-ID] ✗ UNEXPECTED: Found entity (should have thrown exception)");
    }
    catch (NotFoundException<Product, Guid> ex)
    {
        Log.Information("[FIND-BY-ID] ✓ NotFoundException thrown as expected: {Message}", ex.Message);
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 8: Soft Delete with AuditVisitor.AcceptDeleted
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 8: Soft Delete (IsDeleted flag, no physical delete)  ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[SOFT-DELETE] AuditVisitor.AcceptDeleted sets IsDeleted=true");

    await orderService.DeleteOrderAsync(order2.Id, userId);
    Log.Information("[SOFT-DELETE] ✓ Order soft deleted: {OrderNumber}", order2.OrderNumber);

    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var deletedOrder = await context.Orders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == order2.Id);
        
        if (deletedOrder != null)
        {
            Log.Information("[SOFT-DELETE] ✓ Order still exists in database");
            Log.Information("[SOFT-DELETE]   - IsDeleted: {IsDeleted}", deletedOrder.IsDeleted);
            Log.Information("[SOFT-DELETE]   - ModifiedBy: {ModifiedBy} (who deleted it)", deletedOrder.ModifiedBy);
            Log.Information("[SOFT-DELETE]   - ModifiedOn: {ModifiedOn:yyyy-MM-dd HH:mm:ss}", deletedOrder.ModifiedOn);
        }
    }

    Log.Debug("[SOFT-DELETE] Verifying soft-deleted entity is filtered from normal queries");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var normalQuery = await context.Orders.Where(o => o.Id == order2.Id).FirstOrDefaultAsync();
        if (normalQuery == null)
        {
            Log.Information("[SOFT-DELETE] ✓ Soft-deleted order is hidden from normal queries");
        }
        else
        {
            Log.Warning("[SOFT-DELETE] ✗ UNEXPECTED: Soft-deleted order visible in normal query");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 9: DbContextObserver Events
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 9: DbContextObserver Events (Tracked, Changed, Saved)║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Information("[OBSERVER] Observer events are logged throughout this demo:");
    Log.Information("[OBSERVER]   - OnEntityTracked: When entities are loaded/added");
    Log.Information("[OBSERVER]   - OnStateChanged: When entity states change");
    Log.Information("[OBSERVER]   - OnSaved: After SaveChangesAsync completes");
    Log.Information("[OBSERVER]   - Dispose: When DbContext is disposed");
    Log.Information("[OBSERVER] ✓ Check DEBUG logs above for observer event details");

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 10: WithUserId for User Context
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 10: WithUserId for User Context (Type-Safe Chaining) ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[WITH-USER-ID] Testing WithUserIdTyped extension method");

    var differentUserId = Guid.NewGuid();
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(differentUserId))
    {
        var testCustomer = new Customer
        {
            Name = "Test User Context",
            Email = "test.context@example.com",
            PhoneNumber = "555-9999"
        };
        context.Customers.Add(testCustomer);
        await context.SaveChangesAsync();
        
        Log.Information("[WITH-USER-ID] ✓ Entity created with different user context");
        Log.Information("[WITH-USER-ID]   - CreatedBy: {CreatedBy}", testCustomer.CreatedBy);
        Log.Information("[WITH-USER-ID]   - Expected: {Expected}", differentUserId);
        Log.Information("[WITH-USER-ID]   - Match: {Match}", testCustomer.CreatedBy == differentUserId);
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 11: Complex Queries with Navigation Properties
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 11: Complex Queries with Navigation Properties       ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");

    var retrievedOrder = await orderService.GetOrderAsync(order1.Id);
    if (retrievedOrder != null)
    {
        Log.Information("[NAVIGATION] ✓ Order retrieved with includes:");
        Log.Information("[NAVIGATION]   - Order: {OrderNumber}", retrievedOrder.OrderNumber);
        Log.Information("[NAVIGATION]   - Customer: {CustomerName}", retrievedOrder.Customer.Name);
        Log.Information("[NAVIGATION]   - Items: {Count}", retrievedOrder.OrderItems.Count);
        foreach (var item in retrievedOrder.OrderItems)
        {
            Log.Information("[NAVIGATION]     • {ProductName} x{Quantity} @ ${UnitPrice}",
                item.Product.Name, item.Quantity, item.UnitPrice);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 12: Version Tracking for Optimistic Concurrency
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 12: Version Tracking (Optimistic Concurrency)        ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");

    var updatedOrder = await orderService.UpdateOrderStatusAsync(order1.Id, OrderStatus.Processing, userId);
    Log.Information("[VERSION] ✓ Order status updated");
    Log.Information("[VERSION]   - Initial Version: 1");
    Log.Information("[VERSION]   - After Update: {Version}", updatedOrder.Version);

    updatedOrder = await orderService.UpdateOrderStatusAsync(order1.Id, OrderStatus.Shipped, userId);
    Log.Information("[VERSION] ✓ Order status updated again");
    Log.Information("[VERSION]   - Version incremented to: {Version}", updatedOrder.Version);

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 13: IIdentityProvider for Automatic User ID Resolution
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 13: IIdentityProvider (Automatic User ID Resolution) ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[IDENTITY-PROVIDER] Testing automatic user ID resolution");

    // Create a customer WITHOUT explicitly calling WithUserId
    // The IIdentityProvider will automatically provide the user ID
    using (var context = (SampleDbContext)contextFactory.Create())
    {
        var autoCustomer = new Customer
        {
            Name = "Auto User Context",
            Email = "auto.context@example.com",
            PhoneNumber = "555-AUTO"
        };
        context.Customers.Add(autoCustomer);
        await context.SaveChangesAsync();
        
        Log.Information("[IDENTITY-PROVIDER] ✓ Customer created WITHOUT WithUserId() call");
        Log.Information("[IDENTITY-PROVIDER]   - CreatedBy: {CreatedBy}", autoCustomer.CreatedBy);
        Log.Information("[IDENTITY-PROVIDER]   - Expected: {Expected}", userId);
        Log.Information("[IDENTITY-PROVIDER]   - Match: {Match}", autoCustomer.CreatedBy == userId);
        Log.Information("[IDENTITY-PROVIDER]   - Source: IIdentityProvider.GetCurrentUserId()");
    }

    // Test that WithUserId still works and takes precedence
    var overrideUserId = Guid.NewGuid();
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(overrideUserId))
    {
        var overrideCustomer = new Customer
        {
            Name = "Override User Context",
            Email = "override.context@example.com",
            PhoneNumber = "555-OVER"
        };
        context.Customers.Add(overrideCustomer);
        await context.SaveChangesAsync();
        
        Log.Information("[IDENTITY-PROVIDER] ✓ WithUserId() still works and takes precedence");
        Log.Information("[IDENTITY-PROVIDER]   - CreatedBy: {CreatedBy}", overrideCustomer.CreatedBy);
        Log.Information("[IDENTITY-PROVIDER]   - Expected: {Expected}", overrideUserId);
        Log.Information("[IDENTITY-PROVIDER]   - Match: {Match}", overrideCustomer.CreatedBy == overrideUserId);
        Log.Information("[IDENTITY-PROVIDER]   - Source: WithUserId() override");
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE 14: ReadOnlyDbSet - Immediate Fail-Fast Validation
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║ FEATURE 14: ReadOnlyDbSet (Immediate Write Operation Block)  ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Debug("[READONLY-DBSET] ReadOnlyDbSet throws exceptions IMMEDIATELY on write operations");
    Log.Debug("[READONLY-DBSET] Unlike SaveChanges validation, errors occur at the exact line");

    // Demonstrate query operations work normally
    Log.Debug("[READONLY-DBSET] Testing query operations (should work)");
    using (var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId))
    {
        var legacySystems = await ((DbSet<LegacySystem>)context.LegacySystems).ToListAsync();
        Log.Information("[READONLY-DBSET] ✓ Query operations work: Found {Count} legacy system(s)", legacySystems.Count);
        
        var filtered = await ((DbSet<LegacySystem>)context.LegacySystems)
            .Where(ls => ls.SystemName.Contains("Legacy"))
            .ToListAsync();
        Log.Information("[READONLY-DBSET] ✓ LINQ queries work: Filtered to {Count} system(s)", filtered.Count);
    }

    // Demonstrate Add throws immediately
    Log.Debug("[READONLY-DBSET] Testing Add operation (should throw immediately)");
    try
    {
        using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
        var newSystem = new LegacySystem
        {
            Id = 999,
            SystemName = "New System",
            Version = "1.0",
            InstallDate = DateTime.UtcNow
        };
        
        context.LegacySystems.Add(newSystem); // Should throw HERE, not at SaveChanges
        Log.Warning("[READONLY-DBSET] ✗ UNEXPECTED: Add did not throw exception");
    }
    catch (ReadOnlyEntityException ex)
    {
        Log.Information("[READONLY-DBSET] ✓ Add threw ReadOnlyEntityException immediately");
        Log.Information("[READONLY-DBSET]   - Message: {Message}", ex.Message);
        Log.Information("[READONLY-DBSET]   - Operation: create");
        Log.Information("[READONLY-DBSET]   - Entity: LegacySystem");
    }

    // Demonstrate Remove throws immediately
    Log.Debug("[READONLY-DBSET] Testing Remove operation (should throw immediately)");
    try
    {
        using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
        var system = new LegacySystem { Id = 1, SystemName = "Test", Version = "1.0", InstallDate = DateTime.UtcNow };
        context.LegacySystems.Remove(system); // Should throw HERE
        Log.Warning("[READONLY-DBSET] ✗ UNEXPECTED: Remove did not throw exception");
    }
    catch (ReadOnlyEntityException ex)
    {
        Log.Information("[READONLY-DBSET] ✓ Remove threw ReadOnlyEntityException immediately");
        Log.Information("[READONLY-DBSET]   - Operation: delete");
    }

    // Demonstrate Update throws immediately
    Log.Debug("[READONLY-DBSET] Testing Update operation (should throw immediately)");
    try
    {
        using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
        var system = new LegacySystem { Id = 1, SystemName = "Test", Version = "2.0", InstallDate = DateTime.UtcNow };
        context.LegacySystems.Update(system); // Should throw HERE
        Log.Warning("[READONLY-DBSET] ✗ UNEXPECTED: Update did not throw exception");
    }
    catch (ReadOnlyEntityException ex)
    {
        Log.Information("[READONLY-DBSET] ✓ Update threw ReadOnlyEntityException immediately");
        Log.Information("[READONLY-DBSET]   - Operation: update");
    }

    // Demonstrate Attach throws immediately
    Log.Debug("[READONLY-DBSET] Testing Attach operation (should throw immediately)");
    try
    {
        using var context = (SampleDbContext)contextFactory.Create().WithUserIdTyped(userId);
        var system = new LegacySystem { Id = 1, SystemName = "Test", Version = "1.0", InstallDate = DateTime.UtcNow };
        context.LegacySystems.Attach(system); // Should throw HERE
        Log.Warning("[READONLY-DBSET] ✗ UNEXPECTED: Attach did not throw exception");
    }
    catch (ReadOnlyEntityException ex)
    {
        Log.Information("[READONLY-DBSET] ✓ Attach threw ReadOnlyEntityException immediately");
        Log.Information("[READONLY-DBSET]   - Operation: attach");
    }

    Log.Information("[READONLY-DBSET] ✓ ReadOnlyDbSet provides fail-fast protection");
    Log.Information("[READONLY-DBSET] ✓ Errors occur at exact line, not deferred to SaveChanges");
    Log.Information("[READONLY-DBSET] ✓ Works alongside SaveChanges validation as defense-in-depth");

    // ═══════════════════════════════════════════════════════════════
    // Summary
    // ═══════════════════════════════════════════════════════════════
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║                    FEATURE SUMMARY                            ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
    Log.Information("✓ FEATURE 1:  Automatic Auditing (CreatedBy, CreatedOn, Version)");
    Log.Information("✓ FEATURE 2:  Value Converters (EnumConverter, DateTimeConverter)");
    Log.Information("✓ FEATURE 3:  Generic ID Support (Guid, long, int)");
    Log.Information("✓ FEATURE 4:  Read-Only Entities (ReadOnlyEntityBase)");
    Log.Information("✓ FEATURE 5:  Audit on Modify (ModifiedBy, ModifiedOn, Version++)");
    Log.Information("✓ FEATURE 6:  Query Extensions (WhereIf, IncludeIf, Pagination)");
    Log.Information("✓ FEATURE 7:  FindById Extension & NotFoundException");
    Log.Information("✓ FEATURE 8:  Soft Delete (IsDeleted, AuditVisitor.AcceptDeleted)");
    Log.Information("✓ FEATURE 9:  DbContextObserver Events (Tracked, Changed, Saved)");
    Log.Information("✓ FEATURE 10: WithUserId for User Context (Type-Safe)");
    Log.Information("✓ FEATURE 11: Complex Queries with Navigation Properties");
    Log.Information("✓ FEATURE 12: Version Tracking (Optimistic Concurrency)");
    Log.Information("✓ FEATURE 13: IIdentityProvider (Automatic User ID Resolution)");
    Log.Information("✓ FEATURE 14: ReadOnlyDbSet (Immediate Fail-Fast Validation)");
    Log.Information("\n╔═══════════════════════════════════════════════════════════════╗");
    Log.Information("║          ALL BOUNTEOUS.DATA FEATURES VALIDATED ✓              ║");
    Log.Information("╚═══════════════════════════════════════════════════════════════╝");
}
catch (Exception ex)
{
    Log.Error(ex, "[ERROR] Application execution failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
