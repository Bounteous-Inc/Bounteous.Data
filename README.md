# Bounteous.Data

A comprehensive Entity Framework Core data access library for .NET 8+ applications that provides enhanced auditing, connection management, and simplified data operations.

## Architectural Intent

Bounteous.Data is designed to support clean architecture principles and domain-driven design (DDD) patterns by providing a robust data access layer that promotes separation of concerns and loose coupling. This library serves as the foundation for data persistence in enterprise applications, abstracting away the complexities of Entity Framework Core while maintaining flexibility and extensibility.

### Separation of Concerns

The library enforces clear boundaries between different layers of your application:

- **Domain Layer**: Your business entities inherit from `AuditImmutableBase`, focusing purely on business logic and domain rules
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
- [Core Components](#core-components)
- [Usage Examples](#usage-examples)
- [Advanced Features](#advanced-features)
- [API Reference](#api-reference)
- [Contributing](#contributing)

## Features

- **Automatic Auditing**: Built-in audit trail support with `IAuditable` interface
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

public class Customer : AuditImmutableBase
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class Order : AuditImmutableBase
{
    public Guid CustomerId { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
    
    // Navigation property
    public Customer Customer { get; set; } = null!;
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

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure your entity relationships and constraints
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);
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

## Core Components

### AuditImmutableBase

The `AuditImmutableBase` class provides automatic audit fields for your entities:

```csharp
public abstract class AuditImmutableBase : IAuditable, IDeleteable
{
    public Guid Id { get; set; } = Guid.NewGuid();
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

### Basic CRUD Operations

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

### Value Converters

The library includes built-in value converters:

```csharp
// DateTime converter - automatically converts to UTC
public class MyEntity : AuditImmutableBase
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

public class Order : AuditImmutableBase
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

- `IAuditable`: Provides audit fields (Id, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, Version, IsDeleted)
- `IDeleteable`: Provides soft delete capability (IsDeleted)
- `IDbContext`: Extends DbContext with user context support
- `IDbContextObserver`: Entity tracking and state change notifications
- `IConnectionBuilder`: Database connection management
- `IConnectionStringProvider`: Connection string abstraction

### Extension Methods

#### DbContext Extensions
- `CreateNew<TModel, TDomain>()`: Create new auditable entity from model
- `AddNew<TDomain>()`: Add new auditable entity to context
- `DbSet<TDomain>()`: Get typed DbSet for auditable entities

#### Queryable Extensions
- `WhereIf<T>()`: Conditional where clause
- `IncludeIf<T>()`: Conditional include
- `ToPaginatedEnumerableAsync<T>()`: Async paginated enumeration
- `ToPaginatedListAsync<T>()`: Async paginated list

#### Query Extensions
- `FindById<T>()`: Find entity by ID with includes and NotFoundException

#### Enum Extensions
- `GetDescription<T>()`: Get description attribute value
- `FromDescription<T>()`: Convert description to enum value

#### Guid Extensions
- `IsNullOrEmpty()`: Check if Guid is null or empty

### Value Converters

- `DateTimeConverter`: Converts DateTime to UTC
- `EnumConverter<TEnum>`: Converts enum to description string
- `EnumToDescriptionConverter<TEnum>`: Alternative enum converter

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the terms specified in the LICENSE file.
