# Bounteous.Data

A comprehensive Entity Framework Core data access library for .NET 10+ applications that provides enhanced auditing, flexible ID strategies, read-only entity protection, and simplified data operations.

## Overview

Bounteous.Data enhances Entity Framework Core with enterprise-grade features for clean architecture and domain-driven design patterns. It provides automatic auditing, flexible identity strategies, read-only entity protection, and simplified data access patterns.

## Key Features

- ✅ **Automatic Auditing** - CreatedBy, ModifiedBy, Version tracking with zero boilerplate
- ✅ **Flexible ID Strategies** - Support for Guid, long, int for both entities and users
- ✅ **Read-Only Protection** - Two-layer defense (immediate + deferred validation)
- ✅ **ReadOnlyDbSet** - Fail-fast wrapper that throws exceptions immediately on write operations
- ✅ **Automatic User Resolution** - IIdentityProvider integration for seamless auth
- ✅ **Soft Delete Support** - Logical deletion maintaining referential integrity
- ✅ **Query Extensions** - WhereIf, IncludeIf, pagination, FindById helpers
- ✅ **Observer Pattern** - Entity lifecycle events for logging and business rules
- ✅ **Type Safety** - Compile-time checking for ID types and audit fields

## Installation

```bash
dotnet add package Bounteous.Data
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data
```

## Quick Implementation

### 1. Configure Services

```csharp
using Bounteous.Core.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    // Auto-register all services from the assembly
    services.AutoRegister(typeof(Program).Assembly);
    
    // Register your connection string provider
    services.AddSingleton<IConnectionStringProvider, MyConnectionStringProvider>();
    
    // Register your DbContext factory
    services.AddScoped<IDbContextFactory<MyDbContext, Guid>, MyDbContextFactory>();
}
```

### 2. Create Your Entities

```csharp
using Bounteous.Data.Domain.Entities;

// Modern entity with Guid ID and full audit support
public class Customer : AuditBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Legacy entity with long ID
public class LegacyProduct : AuditBase<long, Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Read-only entity (queries only, no CUD operations)
public class LegacySystem : ReadOnlyEntityBase<int>
{
    public string SystemName { get; set; } = string.Empty;
}
```

### 3. Create Your DbContext

```csharp
using Bounteous.Data;
using Bounteous.Data.Domain.ReadOnly;

public class MyDbContext : DbContextBase<Guid>
{
    public MyDbContext(
        DbContextOptions options, 
        IDbContextObserver? observer, 
        IIdentityProvider<Guid> identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    
    // ReadOnlyDbSet for fail-fast protection
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure legacy entities
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
    }
}
```

### 4. Use in Your Services

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext, Guid> contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext, Guid> contextFactory) 
        => this.contextFactory = contextFactory;

    public async Task<Customer> CreateCustomerAsync(string name, string email)
    {
        using var context = contextFactory.Create();
        
        // No need to call WithUserId() - IIdentityProvider handles it automatically!
        var customer = new Customer { Name = name, Email = email };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        // Audit fields automatically populated:
        // - customer.Id = new Guid
        // - customer.CreatedBy = current user ID from IIdentityProvider
        // - customer.CreatedOn = DateTime.UtcNow
        // - customer.Version = 1
        
        return customer;
    }
}
```

## What Gets Automated

When you use `AuditBase` or `AuditBase<TId, TUserId>`, these fields are automatically managed:

```csharp
// On Create:
entity.CreatedBy = currentUserId;      // From IIdentityProvider or WithUserId()
entity.CreatedOn = DateTime.UtcNow;
entity.ModifiedBy = currentUserId;
entity.ModifiedOn = DateTime.UtcNow;
entity.Version = 1;

// On Update:
entity.ModifiedBy = currentUserId;
entity.ModifiedOn = DateTime.UtcNow;
entity.Version++;                      // Incremented for optimistic concurrency
```

## Read-Only Entity Protection

Bounteous.Data provides two layers of protection for read-only entities:

### Layer 1: ReadOnlyEntityBase (Deferred Validation)

```csharp
public class LegacySystem : ReadOnlyEntityBase<int>
{
    public string SystemName { get; set; } = string.Empty;
}

// Error occurs at SaveChanges()
context.LegacySystems.Add(new LegacySystem { Id = 1 });
await context.SaveChangesAsync(); // ❌ Throws ReadOnlyEntityException here
```

### Layer 2: ReadOnlyDbSet (Immediate Validation)

```csharp
public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
    => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

// Error occurs immediately
context.LegacySystems.Add(new LegacySystem { Id = 1 }); // ❌ Throws immediately

// Safe async operations work directly on the ReadOnlyDbSet
var count = await context.LegacySystems.CountAsync();
var first = await context.LegacySystems.FirstOrDefaultAsync(s => s.IsActive);
var hasAny = await context.LegacySystems.AnyAsync(s => s.SystemName.Contains("Legacy"));

// LINQ queries work through IQueryable implementation
var filteredCount = await context.LegacySystems
    .Where(s => s.SystemName.StartsWith("Legacy"))
    .CountAsync();
```

**Benefits:**
- ✅ Fail-fast behavior - errors caught at the point of invalid operation
- ✅ Clear intent - explicit read-only semantics in code
- ✅ Safe async operations - built-in methods for single-value queries
- ✅ Defense in depth - two layers of protection
- ✅ Performance safe - no methods that can load entire tables

### Testing with Read-Only Entities

For unit tests that need to seed read-only entities into an in-memory database, use the fluent API:

```csharp
using Bounteous.Data.Extensions;

[Fact]
public async Task Test_With_ReadOnly_Data()
{
    await using var context = CreateTestContext();
    
    // Suppress validation during test data seeding - fluent API
    using var scope = context.SuppressReadOnlyValidation();
    
    context.LegacySystems.Add(new LegacySystem 
    { 
        Id = 1, 
        SystemName = "Test System" 
    });
    await context.SaveChangesAsync(); // ✅ Succeeds within scope
    
    // Validation automatically re-enabled after scope disposal
}
```

**Alternative syntax:**
```csharp
// Direct instantiation (still supported)
using (new ReadOnlyValidationScope())
{
    // ...
}
```

**Key Points:**
- ✅ Validation is **always enforced by default** in production
- ✅ Scope provides **explicit, self-documenting** test intent
- ✅ Thread-safe using `AsyncLocal<T>` for async operations
- ✅ Automatically re-enables validation when disposed

### Read-Only Request Workflows

For query-only operations (GET endpoints, reports, list views), use the fluent API to enforce that no data modifications occur:

```csharp
using Bounteous.Data.Extensions;

public class CompanyService
{
    public async Task<List<Company>> GetCompaniesAsync()
    {
        await using var context = _contextFactory.Create();
        
        // Enforce read-only mode for this entire request - fluent API
        using var scope = context.EnforceReadOnly();
        
        // Only queries allowed
        return await context.Companies
            .OrderBy(c => c.Name)
            .ToListAsync();
        
        // Any SaveChanges() call will throw InvalidOperationException
    }
}
```

**Alternative syntax:**
```csharp
// Direct instantiation (still supported)
using (new ReadOnlyRequestScope())
{
    // ...
}
```

**Benefits:**
- ✅ **Prevents accidental modifications** in query-only workflows
- ✅ **Fail-fast protection** - catches bugs at SaveChanges
- ✅ **Self-documenting code** - clearly marks read-only operations
- ✅ **Thread-safe** with `AsyncLocal<T>` for concurrent requests

**Comparison:**

| Feature | ReadOnlyValidationScope | ReadOnlyRequestScope |
|---------|------------------------|---------------------|
| Purpose | Suppress validation for test seeding | Enforce read-only for query operations |
| When to use | Unit tests only | Production query endpoints |
| Effect | Allows modifications to readonly entities | Blocks ALL modifications |
| Throws on SaveChanges | No (suppresses) | Yes (enforces) |

## Comprehensive Documentation

For detailed documentation including:
- Complete API reference
- Advanced scenarios (mixed ID strategies, custom observers, etc.)
- IIdentityProvider implementation examples
- Query extensions and helpers
- Value converters
- Best practices and migration guides

**See the [comprehensive developer documentation](docs/README.md)**

## Additional Resources

- [GitHub Repository](https://github.com/Bounteous-Inc/Bounteous.Data)
- [NuGet Package](https://www.nuget.org/packages/Bounteous.Data)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality (xUnit preferred)
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request

## License

This project is licensed under the terms specified in the LICENSE file.
