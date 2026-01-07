# Bounteous.Data

A comprehensive Entity Framework Core data access library for .NET 8+ applications that provides enhanced auditing, flexible ID strategies, read-only entity protection, connection management, and simplified data operations.

## Architectural Intent

Bounteous.Data is designed to support clean architecture principles and domain-driven design (DDD) patterns by providing a robust data access layer that promotes separation of concerns and loose coupling. This library serves as the foundation for data persistence in enterprise applications, abstracting away the complexities of Entity Framework Core while maintaining flexibility and extensibility.

### Separation of Concerns

The library enforces clear boundaries between different layers of your application:

- **Domain Layer**: Your business entities inherit from `AuditBase` or `AuditBase<TId>`, focusing purely on business logic and domain rules
- **Data Layer**: `DbContextBase` handles persistence concerns, audit trails, and database interactions
- **Application Layer**: Services use `IDbContextFactory` to create contexts, maintaining dependency inversion
- **Infrastructure Layer**: Connection management and database configuration are abstracted through interfaces

### Loose Coupling

By leveraging dependency injection and interface-based design, Bounteous.Data ensures that your application components remain loosely coupled:

- Database providers can be swapped without changing business logic
- Connection strings are abstracted through `IConnectionStringProvider`
- Entity tracking and auditing can be customized through `IDbContextObserver`
- Value converters allow for flexible data storage strategies

### Entity Framework Core in Modern Applications

Entity Framework Core serves as the Object-Relational Mapping (ORM) layer in modern .NET applications, bridging the gap between your domain model and relational databases. It provides several key benefits that align with enterprise application architecture:

#### Domain-Driven Design Support

EF Core excels at supporting DDD patterns by:

- **Aggregate Roots**: Entities can be designed as aggregate roots with proper encapsulation
- **Value Objects**: Complex types can be stored as owned entities or using value converters
- **Domain Events**: Change tracking enables domain event publishing
- **Repository Pattern**: DbSet provides a natural repository implementation
- **Unit of Work**: DbContext implements the Unit of Work pattern for transaction management

#### Data Access Simplification

EF Core simplifies data access by:

- **LINQ Integration**: Type-safe queries using familiar LINQ syntax
- **Change Tracking**: Automatic detection of entity modifications
- **Lazy Loading**: On-demand loading of related entities
- **Eager Loading**: Explicit control over data fetching strategies
- **Query Translation**: LINQ expressions translated to efficient SQL

#### Enterprise Features

For enterprise applications, EF Core provides:

- **Connection Resilience**: Built-in retry policies and connection pooling
- **Migration Management**: Database schema versioning and evolution
- **Performance Optimization**: Query optimization and caching strategies
- **Multi-Database Support**: Provider model supporting various database engines
- **Testing Support**: In-memory database for unit testing

### How Bounteous.Data Enhances EF Core

While Entity Framework Core provides excellent ORM capabilities, Bounteous.Data adds enterprise-grade features that are commonly needed in business applications:

- **Automatic Auditing**: Every entity change is automatically tracked with user context and timestamps
- **Soft Delete Support**: Logical deletion without data loss, maintaining referential integrity
- **Consistent Patterns**: Standardized approaches to common data access scenarios
- **Performance Helpers**: Built-in pagination and conditional query extensions
- **Observability**: Entity lifecycle events for logging, caching, and business rule enforcement

This combination allows developers to focus on business logic while ensuring data integrity, audit compliance, and maintainable code architecture.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Entity Base Classes](#entity-base-classes)
  - [Auditable Entities with Guid IDs](#auditable-entities-with-guid-ids)
  - [Auditable Entities with Custom ID Types](#auditable-entities-with-custom-id-types)
  - [Read-Only Entities](#read-only-entities)
  - [Non-Auditable Entities](#non-auditable-entities)
- [Core Components](#core-components)
- [Usage Examples](#usage-examples)
- [Advanced Features](#advanced-features)
- [API Reference](#api-reference)
- [Contributing](#contributing)

## Features

- **Automatic Auditing**: Built-in audit trail support with `IAuditable` and `IAuditable<TId>` interfaces
- **Flexible ID Strategies**: Support for Guid, int, and long primary keys with generic `IEntity<TId>`
- **Read-Only Entities**: Automatic protection against Create, Update, and Delete operations for legacy tables
- **Soft Delete**: Support for soft delete operations with `IDeleteable` interface
- **DbContext Factory Pattern**: Simplified context creation and management
- **Connection Management**: Abstracted connection string and database connection handling
- **Extension Methods**: Rich set of LINQ and DbContext extensions
- **Value Converters**: Built-in converters for DateTime and Enum handling
- **Observer Pattern**: Entity tracking and state change notifications
- **Query Helpers**: Simplified query operations with pagination support

## Installation

Add the Bounteous.Data NuGet package to your project:

```xml
<PackageReference Include="Bounteous.Data" Version="0.0.1" />
```

## Quick Start

### 1. Configure Services

```csharp
using Bounteous.Data;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register the module
    services.AddModule<ModuleStartup>();
    
    // Register your connection string provider
    services.AddSingleton<IConnectionStringProvider, MyConnectionStringProvider>();
    
    // Register your DbContext factory
    services.AddScoped<IDbContextFactory<MyDbContext>, MyDbContextFactory>();
}
```

### 2. Create Your Domain Models

```csharp
using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

// Modern entity with Guid ID and full audit support
public class Customer : AuditBase
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

// Modern entity with Guid ID
public class Order : AuditBase
{
    public Guid CustomerId { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
    
    // Navigation property
    public Customer Customer { get; set; } = null!;
}

// Legacy entity with long ID and audit support
public class LegacyProduct : AuditBase<long>
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
}

// Read-only legacy entity (queries only, no CUD operations)
public class LegacyCustomer : ReadOnlyEntityBase<int>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
}
```

### 3. Create Your DbContext

```csharp
using Bounteous.Data;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    public DbSet<LegacyCustomer> LegacyCustomers { get; set; }

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure your entity relationships and constraints
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);
            
        // Configure legacy entities to not auto-generate IDs
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyCustomer>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
    }
}
```

### 4. Create Your DbContext Factory

```csharp
using Bounteous.Data;
using Microsoft.EntityFrameworkCore;

public class MyDbContextFactory : DbContextFactory<MyDbContext>
{
    public MyDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override MyDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
    {
        return new MyDbContext(options, observer);
    }

    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContextBase>();
        
        if (sensitiveDataLoggingEnabled)
            optionsBuilder.EnableSensitiveDataLogging();

        // Configure your database provider
        optionsBuilder.UseSqlServer(ConnectionBuilder.AdminConnectionString);
        
        return optionsBuilder.Options;
    }
}
```

### 5. Implement Connection String Provider

```csharp
using Bounteous.Data;
using Microsoft.Extensions.Configuration;

public class MyConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public MyConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ConnectionString => _configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string not found");
}
```

## Entity Base Classes

Choose the appropriate base class based on your entity requirements:

### Auditable Entities with Guid IDs

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
// - int Version
// - bool IsDeleted (soft delete support)
```

**When to use:**
- New entities in your application
- Entities requiring full audit trail
- Entities using Guid primary keys

### Auditable Entities with Custom ID Types

Use `AuditBase<TId>` for legacy entities or when you need int/long primary keys:

```csharp
// Legacy entity with long ID
public class LegacyProduct : AuditBase<long>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Legacy entity with int ID
public class LegacyOrder : AuditBase<int>
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

// Provides same audit fields as AuditBase, but with custom ID type
```

**When to use:**
- Legacy database tables with sequence-based IDs
- Integration with existing systems using int/long IDs
- Entities requiring audit support with non-Guid IDs

**Important:** Configure EF Core to not auto-generate IDs for legacy entities:

```csharp
protected override void RegisterModels(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<LegacyProduct>()
        .Property(p => p.Id)
        .ValueGeneratedNever();
}
```

### Read-Only Entities

Use `ReadOnlyEntityBase<TId>` for legacy tables that should only be queried:

```csharp
// Read-only entity with long ID
public class LegacyCustomer : ReadOnlyEntityBase<long>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// Read-only entity with int ID
public class LegacyLookup : ReadOnlyEntityBase<int>
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// Provides:
// - TId Id (custom type)
// - Automatic protection against Create, Update, Delete operations
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

### Non-Auditable Entities

Use `IEntity<TId>` for simple entities without audit support:

```csharp
public class SimpleEntity : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public class LegacyCategory : IEntity<long>
{
    public long Id { get; set; }
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

## Core Components

### AuditBase and AuditBase<TId>

The `AuditBase` classes provide automatic audit field management:

```csharp
// Non-generic version (Guid IDs)
public abstract class AuditBase : AuditBase<Guid>
{
    public AuditBase() => Id = Guid.NewGuid();
}

// Generic version (any ID type)
public abstract class AuditBase<TId> : IAuditable<TId>, IDeleteable
{
    public TId Id { get; set; } = default!;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime SynchronizedOn { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}
```

### DbContextBase

The `DbContextBase` provides:
- Automatic audit field population
- Soft delete support
- Entity tracking notifications
- User context support

### IDbContextObserver

Monitor entity changes and database operations:

```csharp
public interface IDbContextObserver : IDisposable
{
    void OnEntityTracked(object sender, EntityTrackedEventArgs e);
    void OnStateChanged(object? sender, EntityStateChangedEventArgs e);
    void OnSaved();
}
```

## Usage Examples

### Basic CRUD Operations with Guid IDs

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var customer = new Customer 
        { 
            Name = name, 
            Email = email 
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        // Audit fields automatically populated:
        // - customer.Id = new Guid
        // - customer.CreatedBy = userId
        // - customer.CreatedOn = DateTime.UtcNow
        // - customer.Version = 1
        
        return customer;
    }

    public async Task<Customer> GetCustomerAsync(Guid id)
    {
        using var context = _contextFactory.Create();
        
        return await context.Customers.FindById(id);
    }

    public async Task<List<Customer>> GetCustomersAsync(int page = 1, int size = 50)
    {
        using var context = _contextFactory.Create();
        
        return await context.Customers
            .Where(c => !c.IsDeleted)
            .ToPaginatedListAsync(page, size);
    }
}
```

### Working with Legacy Entities (int/long IDs)

```csharp
public class LegacyProductService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public LegacyProductService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LegacyProduct> CreateProductAsync(long id, string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = new LegacyProduct 
        { 
            Id = id,  // Must provide ID for legacy entities
            Name = name,
            Price = price
        };
        
        context.LegacyProducts.Add(product);
        await context.SaveChangesAsync();
        
        // Audit fields still populated automatically
        // - product.CreatedBy = userId
        // - product.CreatedOn = DateTime.UtcNow
        
        return product;
    }

    public async Task<LegacyProduct> GetProductAsync(long id)
    {
        using var context = _contextFactory.Create();
        
        // Generic FindById works with any ID type
        return await context.LegacyProducts.FindById(id);
    }

    public async Task UpdateProductPriceAsync(long id, decimal newPrice, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = await context.LegacyProducts.FindById(id);
        product.Price = newPrice;
        
        await context.SaveChangesAsync();
        
        // Audit fields automatically updated:
        // - product.ModifiedBy = userId
        // - product.ModifiedOn = DateTime.UtcNow
        // - product.Version incremented
    }
}
```

### Querying Read-Only Entities

```csharp
public class LegacyCustomerService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public LegacyCustomerService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ✅ Queries work normally
    public async Task<LegacyCustomer> GetCustomerAsync(int id)
    {
        using var context = _contextFactory.Create();
        return await context.LegacyCustomers.FindAsync(id);
    }

    public async Task<List<LegacyCustomer>> SearchCustomersAsync(string searchTerm)
    {
        using var context = _contextFactory.Create();
        
        return await context.LegacyCustomers
            .Where(c => c.Name.Contains(searchTerm) || c.Email.Contains(searchTerm))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // ❌ This will throw ReadOnlyEntityException
    public async Task CreateCustomerAsync(int id, string name, string email)
    {
        using var context = _contextFactory.Create();
        
        var customer = new LegacyCustomer 
        { 
            Id = id,
            Name = name,
            Email = email
        };
        
        context.LegacyCustomers.Add(customer);
        
        // Throws: ReadOnlyEntityException - "Cannot create read-only entity 'LegacyCustomer'..."
        await context.SaveChangesAsync();
    }
}
```

### Using Extension Methods

```csharp
public async Task<List<Order>> GetOrdersWithConditionalIncludeAsync(Guid customerId, bool includeCustomer)
{
    using var context = _contextFactory.Create();
    
    return await context.Orders
        .Where(o => o.CustomerId == customerId && !o.IsDeleted)
        .IncludeIf(includeCustomer, o => o.Customer)
        .OrderByDescending(o => o.CreatedOn)
        .ToListAsync();
}

public async Task<List<Customer>> SearchCustomersAsync(string? searchTerm, bool activeOnly)
{
    using var context = _contextFactory.Create();
    
    return await context.Customers
        .WhereIf(!string.IsNullOrEmpty(searchTerm), c => c.Name.Contains(searchTerm!))
        .WhereIf(activeOnly, c => !c.IsDeleted)
        .ToListAsync();
}
```

### Soft Delete Operations

```csharp
public async Task DeleteCustomerAsync(Guid customerId, Guid userId)
{
    using var context = _contextFactory.Create().WithUserId(userId);
    
    var customer = await context.Customers.FindById(customerId);
    
    // Soft delete - sets IsDeleted = true
    customer.IsDeleted = true;
    
    await context.SaveChangesAsync();
}
```

### Custom Observer Implementation

```csharp
public class LoggingDbContextObserver : IDbContextObserver
{
    private readonly ILogger<LoggingDbContextObserver> _logger;

    public LoggingDbContextObserver(ILogger<LoggingDbContextObserver> logger)
    {
        _logger = logger;
    }

    public void OnEntityTracked(object sender, EntityTrackedEventArgs e)
    {
        _logger.LogInformation("Entity tracked: {EntityType} with ID {EntityId}", 
            e.Entry.Entity.GetType().Name, 
            e.Entry.Property("Id").CurrentValue);
    }

    public void OnStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        _logger.LogInformation("Entity state changed from {OldState} to {NewState}", 
            e.OldState, e.NewState);
    }

    public void OnSaved()
    {
        _logger.LogInformation("Changes saved to database");
    }

    public void Dispose()
    {
        _logger.LogDebug("DbContextObserver disposed");
    }
}
```

## Advanced Features

### Mixed ID Strategies in Same Application

You can use different ID types in the same application:

```csharp
public class MyDbContext : DbContextBase
{
    // Modern entities with Guid IDs
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    // Legacy entities with long IDs
    public DbSet<LegacyProduct> LegacyProducts { get; set; }
    
    // Legacy entities with int IDs
    public DbSet<LegacyOrder> LegacyOrders { get; set; }
    
    // Read-only legacy entities
    public DbSet<LegacyCustomer> LegacyCustomers { get; set; }
    
    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure legacy entities
        modelBuilder.Entity<LegacyProduct>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyOrder>()
            .Property(o => o.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<LegacyCustomer>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
    }
}

// All entities work together seamlessly
public async Task ProcessOrderAsync(Guid customerId, long productId)
{
    using var context = _contextFactory.Create();
    
    var customer = await context.Customers.FindById(customerId);  // Guid
    var product = await context.LegacyProducts.FindById(productId);  // long
    
    var order = new Order
    {
        CustomerId = customerId,
        Description = $"Order for {product.Name}",
        TotalAmount = product.Price
    };
    
    context.Orders.Add(order);
    await context.SaveChangesAsync();
}
```

### Value Converters

The library includes built-in value converters:

```csharp
// DateTime converter - automatically converts to UTC
public class MyEntity : AuditBase
{
    public DateTime CreatedDate { get; set; } // Automatically converted to UTC
}

// Enum converter - stores enum as description string
public enum OrderStatus
{
    [Description("Pending")]
    Pending,
    [Description("Processing")]
    Processing,
    [Description("Completed")]
    Completed
}

public class Order : AuditBase
{
    public OrderStatus Status { get; set; } // Stored as "Pending", "Processing", etc.
}
```

### Custom Query Extensions

```csharp
public static class CustomerQueryExtensions
{
    public static IQueryable<Customer> Active(this IQueryable<Customer> query)
        => query.Where(c => !c.IsDeleted);

    public static IQueryable<Customer> WithEmail(this IQueryable<Customer> query, string email)
        => query.Where(c => c.Email == email);
}

// Usage
var activeCustomers = await context.Customers
    .Active()
    .WithEmail("test@example.com")
    .ToListAsync();
```

### Pagination Support

```csharp
public async Task<PagedResult<Customer>> GetCustomersPagedAsync(int page = 1, int size = 50)
{
    using var context = _contextFactory.Create();
    
    var query = context.Customers.Where(c => !c.IsDeleted);
    
    var customers = await query.ToPaginatedListAsync(page, size);
    var totalCount = await query.CountAsync();
    
    return new PagedResult<Customer>
    {
        Items = customers,
        TotalCount = totalCount,
        Page = page,
        PageSize = size
    };
}
```

## API Reference

### Core Interfaces

#### Entity Interfaces
- **`IEntity<TId>`**: Base interface providing `TId Id` property for any entity
- **`IAuditable<TId>`**: Extends `IEntity<TId>` with full audit trail support
- **`IAuditable`**: Non-generic version for Guid-based entities (inherits from `IAuditable<Guid>`)
- **`IAuditableMarker`**: Non-generic marker interface for runtime audit detection
- **`IReadOnlyEntity<TId>`**: Marker interface for read-only entities with automatic CUD protection
- **`IDeleteable`**: Provides soft delete capability (`IsDeleted` property)

#### Context Interfaces
- **`IDbContext`**: Extends DbContext with user context support
- **`IDbContextObserver`**: Entity tracking and state change notifications
- **`IConnectionBuilder`**: Database connection management
- **`IConnectionStringProvider`**: Connection string abstraction
- **`IDbContextFactory<TContext>`**: Factory pattern for creating DbContext instances

### Base Classes

#### Entity Base Classes
- **`AuditBase`**: Auditable entity with Guid ID (inherits from `AuditBase<Guid>`)
- **`AuditBase<TId>`**: Generic auditable entity with custom ID type (Guid, int, long)
- **`ReadOnlyEntityBase<TId>`**: Read-only entity with automatic CUD protection

### Extension Methods

#### DbContext Extensions
- **`CreateNew<TModel, TDomain>()`**: Create new auditable entity from model
- **`AddNew<TDomain>()`**: Add new auditable entity to context
- **`DbSet<TDomain>()`**: Get typed DbSet for auditable entities
- **`WithUserId(Guid userId)`**: Set user context for audit tracking

#### Queryable Extensions
- **`WhereIf<T>(bool condition, Expression<Func<T, bool>> predicate)`**: Conditional where clause
- **`IncludeIf<T>(bool condition, Expression<Func<T, object>> navigationProperty)`**: Conditional include
- **`ToPaginatedEnumerableAsync<T>(int page, int size)`**: Async paginated enumeration
- **`ToPaginatedListAsync<T>(int page, int size)`**: Async paginated list

#### Query Extensions
- **`FindById<T>(Guid id)`**: Find entity by Guid ID with NotFoundException
- **`FindById<T, TId>(TId id)`**: Find entity by custom ID type with NotFoundException

#### Enum Extensions
- **`GetDescription<T>()`**: Get description attribute value from enum
- **`FromDescription<T>(string description)`**: Convert description string to enum value

#### Guid Extensions
- **`IsNullOrEmpty()`**: Check if Guid is null or empty

### Exceptions

- **`NotFoundException<T>`**: Thrown when entity with Guid ID is not found
- **`NotFoundException<T, TId>`**: Thrown when entity with custom ID type is not found
- **`ReadOnlyEntityException`**: Thrown when attempting CUD operations on read-only entities

### Value Converters

- **`DateTimeConverter`**: Converts DateTime to UTC for database storage
- **`EnumConverter<TEnum>`**: Converts enum to description string
- **`EnumToDescriptionConverter<TEnum>`**: Alternative enum converter implementation

## Best Practices

### Entity Design

1. **Choose the right base class:**
   - Use `AuditBase` for new Guid-based entities
   - Use `AuditBase<TId>` for legacy entities with int/long IDs
   - Use `ReadOnlyEntityBase<TId>` for tables you can only query
   - Use `IEntity<TId>` for simple entities without audit needs

2. **Configure legacy entity IDs:**
   ```csharp
   modelBuilder.Entity<LegacyEntity>()
       .Property(e => e.Id)
       .ValueGeneratedNever();
   ```

3. **Always set user context for audit tracking:**
   ```csharp
   using var context = _contextFactory.Create().WithUserId(userId);
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

1. **Document why entities are read-only:**
   ```csharp
   /// <summary>
   /// Legacy customer table managed by external system.
   /// Read-only to prevent accidental modifications.
   /// </summary>
   public class LegacyCustomer : ReadOnlyEntityBase<int>
   {
       // ...
   }
   ```

2. **Handle ReadOnlyEntityException appropriately:**
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

1. **Use InMemory database for unit tests:**
   ```csharp
   var options = new DbContextOptionsBuilder<DbContextBase>()
       .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
       .Options;
   ```

2. **Test audit field population:**
   ```csharp
   [Fact]
   public async Task SaveChanges_Should_Populate_Audit_Fields()
   {
       var userId = Guid.NewGuid();
       using var context = new TestDbContext(options, mockObserver.Object)
           .WithUserId(userId);
       
       var customer = new Customer { Name = "Test" };
       context.Customers.Add(customer);
       await context.SaveChangesAsync();
       
       customer.CreatedBy.Should().Be(userId);
       customer.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
   }
   ```

3. **Test read-only protection:**
   ```csharp
   [Fact]
   public async Task ReadOnlyEntity_Should_Throw_On_Create()
   {
       using var context = new TestDbContext(options, mockObserver.Object);
       
       var entity = new ReadOnlyEntity { Id = 123 };
       context.ReadOnlyEntities.Add(entity);
       
       await Assert.ThrowsAsync<ReadOnlyEntityException>(
           async () => await context.SaveChangesAsync());
   }
   ```

## Migration Guide

### From AuditImmutableBase to AuditBase

If you're upgrading from an older version:

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

The functionality is identical - only the class name changed for clarity.

### Adding Generic ID Support to Existing Entities

To convert existing entities to use custom ID types:

```csharp
// Before
public class Product : AuditBase
{
    // Guid Id inherited
}

// After
public class Product : AuditBase<long>
{
    // long Id now
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
