# Bounteous.Data - Comprehensive Developer Documentation

A comprehensive Entity Framework Core data access library for .NET 10+ applications that provides enhanced auditing, flexible ID strategies, read-only entity protection, connection management, and simplified data operations.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Entity Base Classes](#entity-base-classes)
- [Automatic Auditing](#automatic-auditing)
- [Generic ID Support](#generic-id-support)
- [Read-Only Entities](#read-only-entities)
- [ReadOnlyDbSet - Fail-Fast Protection](#readonlydbset---fail-fast-protection)
- [Automatic User ID Resolution](#automatic-user-id-resolution)
- [Query Extensions](#query-extensions)
- [Value Converters](#value-converters)
- [DbContext Observer Pattern](#dbcontext-observer-pattern)
- [Advanced Scenarios](#advanced-scenarios)
- [Best Practices](#best-practices)
- [API Reference](#api-reference)
- [Migration Guide](#migration-guide)

## Features

### Core Auditing & Tracking
- ✅ **Automatic Auditing**: Built-in audit trail with `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn` automatically populated
- ✅ **Version Tracking**: Optimistic concurrency control with automatic version incrementing
- ✅ **Soft Delete Support**: Logical deletion with `IsDeleted` flag, maintaining referential integrity
- ✅ **Automatic User ID Resolution**: `IIdentityProvider<TUserId>` interface for seamless user ID retrieval
- ✅ **User Context Override**: `WithUserId()` method for operation-level user attribution

### Flexible Identity Strategies
- ✅ **Generic User ID Support**: Support for `Guid`, `long`, `int`, or any struct type for user identification
- ✅ **Generic Entity ID Support**: Support for `Guid`, `long`, `int` primary keys with type-safe `IEntity<TId>`
- ✅ **Mixed ID Strategies**: Use different ID types for entities and users in the same application
- ✅ **Type Safety**: Compile-time type checking ensures correct ID types

### Read-Only Entity Protection
- ✅ **ReadOnlyEntityBase**: Base class for query-only entities with automatic CUD protection
- ✅ **ReadOnlyDbSet**: Fail-fast wrapper that throws exceptions immediately on write operations
- ✅ **Defense in Depth**: Two-layer protection (immediate + deferred validation)
- ✅ **Clear Intent**: Explicit read-only semantics in code

### Data Access Patterns
- ✅ **DbContext Factory Pattern**: Simplified context creation and lifecycle management
- ✅ **Connection Management**: Abstracted connection string and database connection handling
- ✅ **Observer Pattern**: Entity lifecycle events for logging, caching, business rules
- ✅ **Unit of Work**: Built-in transaction management through EF Core's `DbContext`

### Query Extensions & Helpers
- ✅ **Conditional Queries**: `WhereIf()` and `IncludeIf()` for dynamic query building
- ✅ **Pagination**: `ToPaginatedListAsync()` with total count and page metadata
- ✅ **FindById Extensions**: Type-safe `FindById<TEntity, TId>()` with automatic `NotFoundException`
- ✅ **LINQ Enhancements**: Rich set of extension methods for common query patterns

### Enterprise Features
- ✅ **Multi-Database Support**: Works with SQL Server, PostgreSQL, MySQL, SQLite
- ✅ **Migration Support**: Design-time DbContext creation for EF Core migrations
- ✅ **Testing Support**: In-memory database support for unit testing
- ✅ **Dependency Injection**: First-class DI support with scoped, singleton, and transient lifetimes
- ✅ **Observability**: Comprehensive logging and event hooks

## Installation

Add the Bounteous.Data NuGet package to your project:

```bash
dotnet add package Bounteous.Data
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="Bounteous.Data" Version="{current.version}" /> <!-- See nuget.org for latest version -->
```

## Quick Start

### 1. Configure Services

```csharp
using Bounteous.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

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

### 2. Create Your Domain Models

```csharp
using Bounteous.Data.Domain.Entities;
using System.ComponentModel.DataAnnotations;

// Modern entity with Guid ID and full audit support
public class Customer : AuditBase
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

// Legacy entity with long ID and audit support
public class LegacyProduct : AuditBase<long, Guid>
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
}

// Read-only legacy entity (queries only, no CUD operations)
public class LegacySystem : ReadOnlyEntityBase<int>
{
    [MaxLength(100)]
    public string SystemName { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
}
```

### 3. Create Your DbContext

```csharp
using Bounteous.Data;
using Bounteous.Data.Domain.ReadOnly;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContextBase<Guid>
{
    // Constructor for migrations
    public MyDbContext(DbContextOptions options, IDbContextObserver? observer)
        : base(options, observer)
    {
    }

    // Constructor with IIdentityProvider for automatic user ID
    public MyDbContext(
        DbContextOptions options, 
        IDbContextObserver? observer, 
        IIdentityProvider<Guid> identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    // Regular DbSets
    public DbSet<Customer> Customers { get; set; }
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    
    // ReadOnlyDbSet for fail-fast protection
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure legacy entities to not auto-generate IDs
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacySystem>()
            .Property(s => s.Id)
            .ValueGeneratedNever();
    }
}
```

### 4. Use Your DbContext

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

## Core Concepts

### Architectural Intent

Bounteous.Data is designed to support clean architecture principles and domain-driven design (DDD) patterns by providing a robust data access layer that promotes separation of concerns and loose coupling.

#### Separation of Concerns

- **Domain Layer**: Business entities inherit from `AuditBase` or `ReadOnlyEntityBase`, focusing on business logic
- **Data Layer**: `DbContextBase` handles persistence, audit trails, and database interactions
- **Application Layer**: Services use `IDbContextFactory` to create contexts, maintaining dependency inversion
- **Infrastructure Layer**: Connection management and database configuration abstracted through interfaces

#### Entity Framework Core in Modern Applications

EF Core serves as the Object-Relational Mapping (ORM) layer, providing:

- **Domain-Driven Design Support**: Aggregate roots, value objects, domain events
- **Data Access Simplification**: LINQ integration, change tracking, lazy/eager loading
- **Enterprise Features**: Connection resilience, migration management, multi-database support

#### How Bounteous.Data Enhances EF Core

While EF Core provides excellent ORM capabilities, Bounteous.Data adds enterprise-grade features:

- **Automatic Auditing**: Every entity change tracked with user context and timestamps
- **Soft Delete Support**: Logical deletion without data loss
- **Read-Only Protection**: Fail-fast validation for query-only entities
- **Consistent Patterns**: Standardized approaches to common data access scenarios
- **Performance Helpers**: Built-in pagination and conditional query extensions
- **Observability**: Entity lifecycle events for logging and monitoring

## Entity Base Classes

Choose the appropriate base class based on your entity requirements:

### AuditBase - Modern Entities with Guid IDs

Use `AuditBase` for new entities with Guid primary keys and full audit support:

```csharp
public class Customer : AuditBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Provides:
// - Guid Id (auto-generated)
// - DateTime CreatedOn, ModifiedOn, SynchronizedOn
// - Guid? CreatedBy, ModifiedBy
// - int Version (optimistic concurrency)
// - bool IsDeleted (soft delete support)
```

**When to use:**
- New entities in your application
- Entities requiring full audit trail
- Entities using Guid primary keys
- Modern applications without legacy constraints

### AuditBase<TId, TUserId> - Custom ID Types

Use `AuditBase<TId, TUserId>` for legacy entities or when you need int/long primary keys:

```csharp
// Legacy entity with long ID and Guid user IDs
public class LegacyProduct : AuditBase<long, Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Legacy entity with int ID and long user IDs
public class LegacyOrder : AuditBase<int, long>
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
```

**When to use:**
- Legacy database tables with sequence-based IDs
- Integration with existing systems using int/long IDs
- Entities requiring audit support with non-Guid IDs
- Mixed ID strategies in the same application

**Important:** Configure EF Core to not auto-generate IDs for legacy entities:

```csharp
protected override void RegisterModels(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<LegacyProduct>()
        .Property(p => p.Id)
        .ValueGeneratedNever();
}
```

### ReadOnlyEntityBase<TId> - Query-Only Entities

Use `ReadOnlyEntityBase<TId>` for legacy tables that should only be queried:

```csharp
// Read-only entity with int ID
public class LegacySystem : ReadOnlyEntityBase<int>
{
    public string SystemName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// Provides:
// - TId Id (custom type)
// - Automatic protection against Create, Update, Delete operations
// - IReadOnlyEntity<TId> marker interface
```

**When to use:**
- Legacy tables you don't have permission to modify
- Reference data managed by other systems
- Historical data that should never change
- Reporting tables or materialized views

**Protection mechanism:**
- ✅ Queries work normally (`ToListAsync()`, `FindAsync()`, `Where()`, etc.)
- ❌ `Add()` + `SaveChangesAsync()` throws `ReadOnlyEntityException`
- ❌ Property modification + `SaveChangesAsync()` throws `ReadOnlyEntityException`
- ❌ `Remove()` + `SaveChangesAsync()` throws `ReadOnlyEntityException`

### IEntity<TId> - Simple Entities

Use `IEntity<TId>` for simple entities without audit support:

```csharp
public class SimpleCategory : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

// Provides:
// - TId Id (any type)
// - No audit fields
// - No soft delete support
```

**When to use:**
- Simple lookup tables
- Entities that don't require audit trails
- High-performance scenarios where audit overhead isn't needed

## Automatic Auditing

Bounteous.Data automatically populates audit fields on all entity changes:

### Audit Fields

All entities inheriting from `AuditBase` or `AuditBase<TId, TUserId>` include:

```csharp
public abstract class AuditBase<TId, TUserId> : IAuditable<TId, TUserId>
    where TUserId : struct
{
    public TId Id { get; set; } = default!;
    public TUserId CreatedBy { get; set; }          // User who created the entity
    public DateTime CreatedOn { get; set; }         // When entity was created (UTC)
    public DateTime SynchronizedOn { get; set; }    // Last sync timestamp (UTC)
    public TUserId ModifiedBy { get; set; }         // User who last modified
    public DateTime ModifiedOn { get; set; }        // When last modified (UTC)
    public int Version { get; set; }                // Optimistic concurrency version
    public bool IsDeleted { get; set; }             // Soft delete flag
}
```

### Automatic Population

Audit fields are automatically populated during `SaveChangesAsync()`:

```csharp
// On Create:
entity.CreatedBy = currentUserId;
entity.CreatedOn = DateTime.UtcNow;
entity.ModifiedBy = currentUserId;
entity.ModifiedOn = DateTime.UtcNow;
entity.SynchronizedOn = DateTime.UtcNow;
entity.Version = 1;

// On Update:
entity.ModifiedBy = currentUserId;
entity.ModifiedOn = DateTime.UtcNow;
entity.SynchronizedOn = DateTime.UtcNow;
entity.Version++; // Incremented for optimistic concurrency
```

### Version Tracking

The `Version` field provides optimistic concurrency control:

```csharp
public async Task UpdateCustomerAsync(Guid id, string newEmail)
{
    using var context = _contextFactory.Create();
    
    var customer = await context.Customers.FindById(id);
    var originalVersion = customer.Version;
    
    customer.Email = newEmail;
    await context.SaveChangesAsync();
    
    // customer.Version is now originalVersion + 1
    // If another user modified the entity, DbUpdateConcurrencyException is thrown
}
```

## Generic ID Support

Bounteous.Data supports flexible ID strategies for both entities and users:

### Identity Strategies

#### 1. Guid-Based IDs (Default)

```csharp
// Entity with Guid entity ID and Guid user ID
public class Customer : AuditBase  // Shorthand for AuditBase<Guid, Guid>
{
    public string Name { get; set; } = string.Empty;
}

// DbContext with Guid user IDs
public class MyDbContext : DbContextBase<Guid>
{
    public DbSet<Customer> Customers { get; set; }
}

// Usage
using var context = _contextFactory.Create().WithUserId(Guid.NewGuid());
```

#### 2. Long-Based User IDs

```csharp
// Entity with Guid entity ID and long user ID
public class Product : AuditBase<Guid, long>
{
    public string Name { get; set; } = string.Empty;
}

// DbContext with long user IDs
public class MyDbContext : DbContextBase<long>
{
    public DbSet<Product> Products { get; set; }
}

// Usage
using var context = _contextFactory.Create().WithUserId(12345L);
```

#### 3. Int-Based User IDs

```csharp
// Entity with Guid entity ID and int user ID
public class Order : AuditBase<Guid, int>
{
    public string OrderNumber { get; set; } = string.Empty;
}

// DbContext with int user IDs
public class MyDbContext : DbContextBase<int>
{
    public DbSet<Order> Orders { get; set; }
}

// Usage
using var context = _contextFactory.Create().WithUserId(42);
```

### Mixed ID Strategies

You can mix different **entity ID types** in the same context, but all entities must use the **same user ID type**:

```csharp
// Context with long user IDs
public class MyDbContext : DbContextBase<long>
{
    // ✅ Guid entity ID + long user ID
    public DbSet<Customer> Customers { get; set; }  // AuditBase<Guid, long>
    
    // ✅ Long entity ID + long user ID
    public DbSet<Product> Products { get; set; }  // AuditBase<long, long>
    
    // ✅ Int entity ID + long user ID
    public DbSet<Order> Orders { get; set; }  // AuditBase<int, long>
}
```

### Type Safety

The library enforces type safety at compile time:

```csharp
// ✅ Correct - entity user ID matches context user ID
public class MyDbContext : DbContextBase<long>
{
    public DbSet<Product> Products { get; set; }  // Product uses long user IDs
}
public class Product : AuditBase<Guid, long> { }

// ❌ Compile error - entity user ID doesn't match context
public class MyDbContext : DbContextBase<long>
{
    public DbSet<Customer> Customers { get; set; }  // Customer uses Guid user IDs
}
public class Customer : AuditBase<Guid, Guid> { }  // Won't be audited!
```

## Read-Only Entities

Bounteous.Data provides two complementary layers of read-only protection:

### Layer 1: ReadOnlyEntityBase (Deferred Validation)

Base class that marks entities as read-only, validated during `SaveChanges()`:

```csharp
public class LegacySystem : ReadOnlyEntityBase<int>
{
    public string SystemName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// Usage
using var context = _contextFactory.Create();

// ✅ Queries work normally
var systems = await context.LegacySystems.ToListAsync();
var system = await context.LegacySystems.FindAsync(123);

// ❌ Write operations are tracked but throw at SaveChanges
context.LegacySystems.Add(new LegacySystem { Id = 1 });
await context.SaveChangesAsync(); // Throws ReadOnlyEntityException here
```

**Characteristics:**
- Error occurs at `SaveChanges()`, not at the point of Add/Remove/Update
- Stack trace points to `SaveChanges()` call
- Serves as a safety net for all write operations

### Layer 2: ReadOnlyDbSet (Immediate Validation)

Wrapper that throws exceptions immediately when write operations are attempted:

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    // Return ReadOnlyDbSet from property for fail-fast protection
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
}

// Usage
using var context = _contextFactory.Create();

// ✅ Queries work normally
var systems = await context.LegacySystems.ToListAsync();
var filtered = await context.LegacySystems
    .Where(s => s.SystemName.Contains("Legacy"))
    .ToListAsync();

// ❌ Write operations throw immediately
context.LegacySystems.Add(new LegacySystem { Id = 1 }); // Throws ReadOnlyEntityException HERE
```

**Characteristics:**
- Error occurs immediately at Add/Remove/Update call
- Stack trace points to the exact line of invalid operation
- Provides fail-fast behavior and clear developer feedback

## ReadOnlyDbSet - Fail-Fast Protection

`ReadOnlyDbSet<TEntity, TId>` is a lightweight wrapper around `DbSet<T>` that provides immediate validation for read-only entities.

### Design Philosophy

**Two-Layer Protection Strategy:**

1. **Immediate validation** (ReadOnlyDbSet) - Throws exceptions at the point of Add/Remove/Update calls
2. **Deferred validation** (DbContextBase) - Validates during SaveChanges as a safety net

This defense-in-depth approach ensures read-only entities are protected at multiple levels.

### Architecture

```
IReadOnlyEntity<TId>
    ↑
    |
ReadOnlyEntityBase<TId>
    ↑
    |
Your Entity (e.g., LegacySystem)
    ↓
DbSet<TEntity>
    ↓
ReadOnlyDbSet<TEntity, TId> (wrapper with implicit conversion)
```

### Implementation

The wrapper uses **composition + implicit conversion**:

```csharp
public class ReadOnlyDbSet<TEntity, TId> where TEntity : class, IReadOnlyEntity<TId>
{
    private readonly DbSet<TEntity> innerDbSet;

    // Implicit conversion to DbSet for query operations
    public static implicit operator DbSet<TEntity>(ReadOnlyDbSet<TEntity, TId> readOnlySet)
        => readOnlySet.innerDbSet;

    // All write operations throw immediately
    public EntityEntry<TEntity> Add(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "create");
    
    // ... other write operations
}
```

### Usage Patterns

#### Option 1: DbContext Property (Recommended)

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    // Return ReadOnlyDbSet directly from property
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
}

// Usage - queries work seamlessly
var systems = await context.LegacySystems.ToListAsync();
var system = await context.LegacySystems.FindAsync(123);

// Write operations throw immediately
context.LegacySystems.Add(system); // ❌ ReadOnlyEntityException thrown here
```

#### Option 2: On-Demand Conversion

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    public DbSet<LegacySystem> LegacySystemsSet { get; set; }
}

// Convert to ReadOnlyDbSet when needed
var readOnlySet = context.LegacySystemsSet.AsReadOnly<LegacySystem, int>();
var systems = await readOnlySet.ToListAsync();
```

### Query Operations (Allowed)

All query operations work normally through implicit conversion:

```csharp
// Basic queries
var all = await context.LegacySystems.ToListAsync();
var one = await context.LegacySystems.FindAsync(123);

// LINQ queries
var filtered = await context.LegacySystems
    .Where(s => s.SystemName.Contains("Legacy"))
    .OrderBy(s => s.CreatedDate)
    .Skip(10)
    .Take(20)
    .ToListAsync();

// Async enumeration
await foreach (var system in context.LegacySystems.AsAsyncEnumerable())
{
    Console.WriteLine(system.SystemName);
}

// IQueryable operations
var queryable = context.LegacySystems.AsQueryable();
```

### Write Operations (Blocked)

All write operations throw `ReadOnlyEntityException` **immediately**:

```csharp
var readOnlySet = context.LegacySystems;

// ❌ All these throw immediately
readOnlySet.Add(system);
await readOnlySet.AddAsync(system);
readOnlySet.AddRange(systems);
await readOnlySet.AddRangeAsync(systems);

readOnlySet.Remove(system);
readOnlySet.RemoveRange(systems);

readOnlySet.Update(system);
readOnlySet.UpdateRange(systems);

readOnlySet.Attach(system);
readOnlySet.AttachRange(systems);
```

### Benefits

1. **Fail-Fast Behavior**: Errors caught immediately at the point of invalid operation
2. **Clear Intent**: Using `ReadOnlyDbSet` makes read-only semantics explicit
3. **Better Developer Experience**: IDE autocomplete shows only valid operations
4. **Defense in Depth**: Works alongside `SaveChanges()` validation as a second layer
5. **Zero Performance Overhead**: Implicit conversion means no runtime cost for queries

### Comparison Table

| Aspect | ReadOnlyDbSet | SaveChanges Validation |
|--------|---------------|------------------------|
| **When** | Immediate (at Add/Remove/Update) | Deferred (at SaveChanges) |
| **Error Location** | Exact line of invalid operation | Inside SaveChanges |
| **Stack Trace** | Points to problematic code | Points to SaveChanges |
| **Prevention** | Prevents tracking entirely | Validates tracked entities |
| **Use Case** | Proactive protection | Safety net |
| **Developer Feedback** | Immediate, clear | Delayed, less clear |

### Best Practices

1. **Use ReadOnlyDbSet for properties** - Return `ReadOnlyDbSet<T, TId>` from DbContext properties
2. **Keep SaveChanges validation** - Don't remove deferred validation; it's a safety net
3. **Consistent naming** - Use clear names like `LegacySystems` to indicate read-only intent
4. **Document intent** - Add XML comments explaining why entities are read-only

```csharp
/// <summary>
/// Legacy system table managed by external application.
/// Read-only to prevent accidental modifications.
/// </summary>
public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
    => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
```

### Handling Explicit Casts

For some operations, you may need explicit casts:

```csharp
// When calling DbSet-specific methods
DbSet<LegacySystem> dbSet = context.LegacySystems;
var local = dbSet.Local; // Access Local collection

// When calling extension methods that don't work with implicit conversion
var paginated = await ((DbSet<LegacySystem>)context.LegacySystems)
    .ToPaginatedListAsync(page: 1, size: 50);
```

## Automatic User ID Resolution

The `IIdentityProvider<TUserId>` interface enables automatic user ID resolution from your authentication context, eliminating the need to manually call `WithUserId()` before every `SaveChanges()`.

### Interface Definition

```csharp
public interface IIdentityProvider<TUserId> where TUserId : struct
{
    TUserId GetCurrentUserId();
}
```

### Benefits

- ✅ **Automatic user tracking** - No need to remember to call `WithUserId()`
- ✅ **Centralized auth logic** - User ID retrieval in one place
- ✅ **Cleaner service code** - Less boilerplate in repositories/services
- ✅ **Flexible** - Can still override with `WithUserId()` when needed
- ✅ **Type-safe** - Works with `long`, `Guid`, `int`, or any struct type

### Implementation Examples

#### ASP.NET Core with HttpContext

```csharp
using Bounteous.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class HttpContextIdentityProvider : IIdentityProvider<Guid>
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpContextIdentityProvider(IHttpContextAccessor httpContextAccessor) 
        => this.httpContextAccessor = httpContextAccessor;

    public Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("sub")
            ?? httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        
        return userIdClaim?.Value is string userId && Guid.TryParse(userId, out var id) 
            ? id 
            : default; // No authenticated user
    }
}
```

#### Custom Authentication Service

```csharp
public class CustomIdentityProvider : IIdentityProvider<long>
{
    private readonly ICurrentUserService currentUserService;

    public CustomIdentityProvider(ICurrentUserService currentUserService) 
        => this.currentUserService = currentUserService;

    public long GetCurrentUserId() => currentUserService.GetUserId();
}
```

### Service Registration

```csharp
// Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Required for HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register your IIdentityProvider implementation
builder.Services.AddScoped<IIdentityProvider<Guid>, HttpContextIdentityProvider>();

// Register DbContext factory with IIdentityProvider
builder.Services.AddScoped<IDbContextFactory<MyDbContext, Guid>, MyDbContextFactory>();
```

### Usage in Services

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext, Guid> contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext, Guid> contextFactory) 
        => this.contextFactory = contextFactory;

    public async Task<Customer> CreateCustomerAsync(string name, string email)
    {
        using var context = contextFactory.Create();
        
        // No WithUserId() needed - IIdentityProvider handles it automatically!
        var customer = new Customer { Name = name, Email = email };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        // customer.CreatedBy is automatically set from IIdentityProvider
        return customer;
    }
}
```

### Fallback Behavior

The `DbContext` uses this priority order for determining the user ID:

1. **`WithUserId()` override** - Operation-level user attribution (highest priority)
2. **`IIdentityProvider`** - Application-level authenticated user context
3. **Default value** - `default(TUserId)` if neither is available

### WithUserId() vs IIdentityProvider

Both mechanisms serve distinct but complementary purposes:

#### IIdentityProvider - Application-Level Context

**Purpose**: Represents the authenticated user making the request

**Use Case**: "Who is the authenticated user making this request?"

```csharp
// Web API - User authenticated via JWT
// IIdentityProvider.GetCurrentUserId() returns the authenticated user's ID
public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
{
    using var context = _factory.Create();
    
    // No WithUserId() needed - IIdentityProvider provides it automatically
    var customer = new Customer { Name = request.Name };
    context.Customers.Add(customer);
    await context.SaveChangesAsync();
    
    // customer.CreatedBy is set to the authenticated user's ID
    return Ok(customer);
}
```

#### WithUserId() - Operation-Level Override

**Purpose**: Override user attribution for specific operations

**Use Case**: "For THIS specific operation, attribute it to a different user"

```csharp
// Admin creating order on behalf of customer
public async Task<Order> CreateOrderOnBehalfOfCustomer(Guid customerId, OrderRequest request)
{
    using var context = _factory.Create();
    
    // Admin is authenticated (IIdentityProvider returns admin's ID)
    // But we want the order attributed to the customer
    context.WithUserId(customerId);
    
    var order = new Order { /* ... */ };
    context.Orders.Add(order);
    await context.SaveChangesAsync();
    
    // order.CreatedBy = customerId (not the admin's ID)
    return order;
}
```

### Real-World Use Cases for WithUserId()

#### 1. Admin Impersonation

```csharp
// Admin (ID: 123) creating data on behalf of customer (ID: 456)
public async Task AdminCreateCustomerDataAsync(Guid customerId, string data)
{
    using var context = _factory.Create().WithUserId(customerId);
    
    // IIdentityProvider returns admin ID
    // But CreatedBy will be customer ID
    var entity = new CustomerData { Data = data };
    context.CustomerData.Add(entity);
    await context.SaveChangesAsync();
}
```

#### 2. Background Jobs with User Context

```csharp
// Background job processing user-specific data
public async Task ProcessUserDataBatchAsync(List<Guid> userIds)
{
    foreach (var userId in userIds)
    {
        using var context = _factory.Create().WithUserId(userId);
        
        // Each operation gets correct user attribution
        var data = await ProcessDataForUser(userId);
        context.ProcessedData.Add(data);
        await context.SaveChangesAsync();
        
        // data.CreatedBy = userId (not the background service account)
    }
}
```

#### 3. System Operations

```csharp
// System-initiated operations (e.g., automated cleanup)
public async Task SystemCleanupAsync()
{
    var SYSTEM_USER_ID = Guid.Empty;
    using var context = _factory.Create().WithUserId(SYSTEM_USER_ID);
    
    // Mark old records as deleted by system
    var oldRecords = await context.Records
        .Where(r => r.CreatedOn < DateTime.UtcNow.AddYears(-5))
        .ToListAsync();
    
    context.Records.RemoveRange(oldRecords);
    await context.SaveChangesAsync();
    
    // All deletions attributed to SYSTEM_USER_ID
}
```

## Query Extensions

Bounteous.Data provides a rich set of query extension methods for common patterns:

### Conditional Queries

#### WhereIf - Conditional Filtering

```csharp
public async Task<List<Customer>> SearchCustomersAsync(
    string? searchTerm, 
    bool activeOnly)
{
    using var context = _contextFactory.Create();
    
    return await context.Customers
        .WhereIf(!string.IsNullOrEmpty(searchTerm), c => c.Name.Contains(searchTerm!))
        .WhereIf(activeOnly, c => !c.IsDeleted)
        .ToListAsync();
}
```

#### IncludeIf - Conditional Eager Loading

```csharp
public async Task<List<Order>> GetOrdersAsync(
    Guid customerId, 
    bool includeCustomer, 
    bool includeItems)
{
    using var context = _contextFactory.Create();
    
    return await context.Orders
        .Where(o => o.CustomerId == customerId)
        .IncludeIf(includeCustomer, o => o.Customer)
        .IncludeIf(includeItems, o => o.OrderItems)
        .ToListAsync();
}
```

### Pagination

#### ToPaginatedListAsync

```csharp
public async Task<List<Customer>> GetCustomersPageAsync(int page = 1, int size = 50)
{
    using var context = _contextFactory.Create();
    
    return await context.Customers
        .Where(c => !c.IsDeleted)
        .OrderBy(c => c.Name)
        .ToPaginatedListAsync(page, size);
}
```

#### ToPaginatedEnumerableAsync

```csharp
public async Task<IEnumerable<Customer>> GetCustomersEnumerableAsync(int page, int size)
{
    using var context = _contextFactory.Create();
    
    return await context.Customers
        .Where(c => !c.IsDeleted)
        .ToPaginatedEnumerableAsync(page, size);
}
```

### FindById Extensions

Type-safe entity lookup with automatic `NotFoundException`:

```csharp
// Guid ID
public async Task<Customer> GetCustomerAsync(Guid id)
{
    using var context = _contextFactory.Create();
    
    // Throws NotFoundException<Customer> if not found
    return await context.Customers.FindById(id);
}

// Custom ID type
public async Task<LegacyProduct> GetProductAsync(long id)
{
    using var context = _contextFactory.Create();
    
    // Throws NotFoundException<LegacyProduct, long> if not found
    return await context.LegacyProducts.FindById(id);
}
```

## Value Converters

Bounteous.Data includes built-in value converters for common scenarios:

### DateTime Converter

Automatically converts DateTime values to UTC for database storage:

```csharp
public class MyEntity : AuditBase
{
    public DateTime EventDate { get; set; } // Automatically converted to UTC
}

// Usage
var entity = new MyEntity 
{ 
    EventDate = DateTime.Now // Stored as UTC in database
};
```

### Enum Converter

Stores enums as description strings in the database:

```csharp
public enum OrderStatus
{
    [Description("Pending")]
    Pending,
    
    [Description("Processing")]
    Processing,
    
    [Description("Completed")]
    Completed,
    
    [Description("Cancelled")]
    Cancelled
}

public class Order : AuditBase
{
    public OrderStatus Status { get; set; } // Stored as "Pending", "Processing", etc.
}

// Usage
var order = new Order { Status = OrderStatus.Pending };
// Database stores "Pending" as string
```

### Custom Converters

You can create custom value converters by implementing EF Core's `ValueConverter<T>`:

```csharp
public class MyCustomConverter : ValueConverter<MyType, string>
{
    public MyCustomConverter()
        : base(
            v => v.ToString(),
            v => MyType.Parse(v))
    {
    }
}

// Register in DbContext
protected override void RegisterModels(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MyEntity>()
        .Property(e => e.MyProperty)
        .HasConversion<MyCustomConverter>();
}
```

## DbContext Observer Pattern

The `IDbContextObserver` interface provides hooks into entity lifecycle events:

### Interface Definition

```csharp
public interface IDbContextObserver : IDisposable
{
    void OnEntityTracked(object sender, EntityTrackedEventArgs e);
    void OnStateChanged(object? sender, EntityStateChangedEventArgs e);
    void OnSaved();
}
```

### Implementation Example

```csharp
public class LoggingDbContextObserver : IDbContextObserver
{
    private readonly ILogger<LoggingDbContextObserver> logger;

    public LoggingDbContextObserver(ILogger<LoggingDbContextObserver> logger) 
        => this.logger = logger;

    public void OnEntityTracked(object sender, EntityTrackedEventArgs e)
    {
        logger.LogInformation(
            "Entity tracked: {EntityType} with ID {EntityId}", 
            e.Entry.Entity.GetType().Name, 
            e.Entry.Property("Id").CurrentValue);
    }

    public void OnStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        logger.LogInformation(
            "Entity state changed from {OldState} to {NewState}", 
            e.OldState, 
            e.NewState);
    }

    public void OnSaved()
    {
        logger.LogInformation("Changes saved to database");
    }

    public void Dispose()
    {
        logger.LogDebug("DbContextObserver disposed");
    }
}
```

### Use Cases

- **Logging**: Track all entity changes for audit logs
- **Caching**: Invalidate cache entries when entities change
- **Business Rules**: Enforce cross-entity business rules
- **Event Publishing**: Publish domain events on entity changes
- **Monitoring**: Track database operation metrics

## Advanced Scenarios

### Mixed ID Strategies

Use different entity ID types in the same application:

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    // Modern entities with Guid IDs
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    // Legacy entities with long IDs (but Guid user IDs)
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    
    // Legacy entities with int IDs (but Guid user IDs)
    public DbSet<LegacyOrder> LegacyOrders { get; set; }
    
    // Read-only legacy entities
    public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
        => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
    
    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure legacy entities
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyOrder>()
            .Property(o => o.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacySystem>()
            .Property(s => s.Id)
            .ValueGeneratedNever();
    }
}
```

### Soft Delete Queries

Filter soft-deleted entities in queries:

```csharp
public async Task<List<Customer>> GetActiveCustomersAsync()
{
    using var context = _contextFactory.Create();
    
    return await context.Customers
        .Where(c => !c.IsDeleted)
        .OrderBy(c => c.Name)
        .ToListAsync();
}

public async Task<List<Customer>> GetAllCustomersIncludingDeletedAsync()
{
    using var context = _contextFactory.Create();
    
    // Include soft-deleted entities
    return await context.Customers
        .OrderBy(c => c.Name)
        .ToListAsync();
}
```

### Custom Query Extensions

Create domain-specific query extensions:

```csharp
public static class CustomerQueryExtensions
{
    public static IQueryable<Customer> Active(this IQueryable<Customer> query) 
        => query.Where(c => !c.IsDeleted);

    public static IQueryable<Customer> WithEmail(this IQueryable<Customer> query, string email) 
        => query.Where(c => c.Email == email);
    
    public static IQueryable<Customer> CreatedAfter(this IQueryable<Customer> query, DateTime date) 
        => query.Where(c => c.CreatedOn >= date);
}

// Usage
var recentActiveCustomers = await context.Customers
    .Active()
    .CreatedAfter(DateTime.UtcNow.AddMonths(-1))
    .ToListAsync();
```

### Testing with InMemory Database

```csharp
[Fact]
public async Task CreateCustomer_Should_Populate_Audit_Fields()
{
    // Arrange
    var options = new DbContextOptionsBuilder<MyDbContext>()
        .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
        .Options;
    
    var mockObserver = new Mock<IDbContextObserver>();
    var userId = Guid.NewGuid();
    
    // Act
    using (var context = new MyDbContext(options, mockObserver.Object))
    {
        context.WithUserId(userId);
        
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        // Assert
        customer.CreatedBy.Should().Be(userId);
        customer.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.Version.Should().Be(1);
    }
}
```

## Best Practices

### Entity Design

1. **Choose the right base class:**
   - Use `AuditBase` for new Guid-based entities
   - Use `AuditBase<TId, TUserId>` for legacy entities with int/long IDs
   - Use `ReadOnlyEntityBase<TId>` for tables you can only query
   - Use `IEntity<TId>` for simple entities without audit needs

2. **Configure legacy entity IDs:**
   ```csharp
   modelBuilder.Entity<LegacyEntity>()
       .Property(e => e.Id)
       .ValueGeneratedNever();
   ```

3. **Use IIdentityProvider for automatic user tracking:**
   ```csharp
   // No need to call WithUserId() manually
   using var context = _contextFactory.Create();
   ```

4. **Override with WithUserId() when needed:**
   ```csharp
   // For admin impersonation, background jobs, etc.
   using var context = _contextFactory.Create().WithUserId(specificUserId);
   ```

### Query Optimization

1. **Use conditional includes to avoid over-fetching:**
   ```csharp
   var orders = await context.Orders
       .IncludeIf(includeCustomer, o => o.Customer)
       .ToListAsync();
   ```

2. **Filter soft-deleted entities:**
   ```csharp
   var activeCustomers = await context.Customers
       .Where(c => !c.IsDeleted)
       .ToListAsync();
   ```

3. **Use pagination for large result sets:**
   ```csharp
   var page = await context.Customers
       .ToPaginatedListAsync(pageNumber, pageSize);
   ```

### Read-Only Entity Guidelines

1. **Use ReadOnlyDbSet for DbContext properties:**
   ```csharp
   public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
       => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
   ```

2. **Document why entities are read-only:**
   ```csharp
   /// <summary>
   /// Legacy customer table managed by external system.
   /// Read-only to prevent accidental modifications.
   /// </summary>
   public class LegacyCustomer : ReadOnlyEntityBase<int> { }
   ```

3. **Handle ReadOnlyEntityException appropriately:**
   ```csharp
   try
   {
       await context.SaveChangesAsync();
   }
   catch (ReadOnlyEntityException ex)
   {
       _logger.LogError(ex, "Attempted to modify read-only entity");
       throw new InvalidOperationException("Cannot modify legacy data", ex);
   }
   ```

### Testing

1. **Use InMemory database for unit tests**
2. **Test audit field population**
3. **Test read-only protection**
4. **Test optimistic concurrency with version tracking**
5. **Test soft delete behavior**

## API Reference

### Core Interfaces

#### Entity Interfaces
- `IEntity<TId>` - Base interface providing `TId Id` property
- `IAuditable<TId, TUserId>` - Full audit trail support with generic IDs
- `IAuditableMarker<TUserId>` - Generic marker interface for runtime audit detection
- `IReadOnlyEntity<TId>` - Marker interface for read-only entities
- `IDeleteable` - Soft delete capability (`IsDeleted` property)

#### Context Interfaces
- `IDbContext<TUserId>` - Generic interface extending DbContext with user context
- `IIdentityProvider<TUserId>` - Automatic user ID resolution from authentication
- `IDbContextObserver` - Entity tracking and state change notifications
- `IConnectionBuilder` - Database connection management
- `IConnectionStringProvider` - Connection string abstraction
- `IDbContextFactory<TContext, TUserId>` - Factory pattern for creating DbContext instances

### Base Classes

#### Entity Base Classes
- `AuditBase` - Convenience wrapper for Guid-based auditable entities
- `AuditBase<TId, TUserId>` - Generic auditable entity with custom ID types
- `ReadOnlyEntityBase<TId>` - Read-only entity with automatic CUD protection

#### Context Base Classes
- `DbContextBase<TUserId>` - Generic DbContext with audit support and user context

### Extension Methods

#### DbContext Extensions
- `WithUserId(TUserId userId)` - Set user context for audit tracking
- `CreateNew<TModel, TDomain>()` - Create new auditable entity from model
- `AddNew<TDomain>()` - Add new auditable entity to context

#### Queryable Extensions
- `WhereIf<T>(bool condition, Expression<Func<T, bool>> predicate)` - Conditional where clause
- `IncludeIf<T>(bool condition, Expression<Func<T, object>> navigationProperty)` - Conditional include
- `ToPaginatedEnumerableAsync<T>(int page, int size)` - Async paginated enumeration
- `ToPaginatedListAsync<T>(int page, int size)` - Async paginated list

#### Query Extensions
- `FindById<T>(Guid id)` - Find entity by Guid ID with NotFoundException
- `FindById<T, TId>(TId id)` - Find entity by custom ID type with NotFoundException
- `AsReadOnly<TEntity, TId>()` - Convert DbSet to ReadOnlyDbSet

### Exceptions

- `NotFoundException<T>` - Thrown when entity with Guid ID is not found
- `NotFoundException<T, TId>` - Thrown when entity with custom ID type is not found
- `ReadOnlyEntityException` - Thrown when attempting CUD operations on read-only entities

### Value Converters

- `DateTimeConverter` - Converts DateTime to UTC for database storage
- `EnumConverter<TEnum>` - Converts enum to description string
- `EnumToDescriptionConverter<TEnum>` - Alternative enum converter implementation

## Migration Guide

### From Version 0.0.20 to 0.0.21

#### Namespace Changes

The domain classes have been reorganized into subdirectories:

```csharp
// Old namespaces
using Bounteous.Data.Domain;

// New namespaces
using Bounteous.Data.Domain.Entities;      // AuditBase, etc.
using Bounteous.Data.Domain.Interfaces;    // IEntity, IAuditable, etc.
using Bounteous.Data.Domain.ReadOnly;      // ReadOnlyEntityBase, ReadOnlyDbSet
```

#### ReadOnlyDbSet Feature

New fail-fast protection for read-only entities:

```csharp
// Before (deferred validation only)
public DbSet<LegacySystem> LegacySystems { get; set; }

// After (immediate validation)
public ReadOnlyDbSet<LegacySystem, int> LegacySystems 
    => Set<LegacySystem>().AsReadOnly<LegacySystem, int>();
```

#### Auto-Registration

Use Bounteous.Core's auto-registration:

```csharp
// Before
services.AddScoped<ICustomerService, CustomerService>();
services.AddScoped<IOrderService, OrderService>();
// ... many more registrations

// After
services.AutoRegister(typeof(Program).Assembly);
```

### From AuditImmutableBase to AuditBase

```csharp
// Old
public class Customer : AuditImmutableBase
{
    // ...
}

// New (no code changes needed, just rename)
public class Customer : AuditBase
{
    // ...
}
```

### Adding Generic ID Support

```csharp
// Before
public class Product : AuditBase
{
    // Guid Id inherited
}

// After
public class Product : AuditBase<long, Guid>
{
    // long entity ID, Guid user ID
}

// Update DbContext configuration
protected override void RegisterModels(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .Property(p => p.Id)
        .ValueGeneratedNever();  // Important for non-Guid IDs
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality (xUnit preferred)
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request

## License

This project is licensed under the terms specified in the LICENSE file.

## Additional Resources

- [ReadOnlyDbSet Documentation](ReadOnlyDbSet.md) - Detailed ReadOnlyDbSet implementation guide
- [GitHub Repository](https://github.com/Bounteous-Inc/Bounteous.Data)
- [NuGet Package](https://www.nuget.org/packages/Bounteous.Data)
