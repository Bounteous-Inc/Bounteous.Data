# Bounteous.Data.Sample

A reference application demonstrating the usage of the Bounteous.Data module with an in-memory database.

## Overview

This sample application showcases:
- **Domain-Driven Design**: Fictitious e-commerce domain with Customer, Product, Order, and OrderItem entities
- **Automatic Auditing**: All entities inherit from `AuditBase` for automatic audit trail tracking
- **In-Memory Database**: Uses Entity Framework Core's in-memory provider for demonstration
- **IoC Registration**: Uses Bounteous.Core's `IAppStartup` pattern for dependency injection
- **Service Layer**: Clean separation with service interfaces and implementations
- **CRUD Operations**: Complete Create, Read, Update, Delete operations with audit tracking
- **Soft Delete**: Demonstrates soft delete functionality

## Project Structure

```
Bounteous.Data.Sample/
├── Domain/                      # Domain entities
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── Data/                        # Data access layer
│   ├── SampleDbContext.cs
│   └── SampleDbContextFactory.cs
├── Infrastructure/              # Infrastructure services
│   └── SampleConnectionStringProvider.cs
├── Services/                    # Business services
│   ├── ICustomerService.cs
│   ├── CustomerService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IOrderService.cs
│   └── OrderService.cs
├── AppStartup.cs               # IoC registration
├── ApplicationConfig.cs        # Application configuration
└── Program.cs                  # Entry point with demonstration
```

## Domain Model

### Customer
- **Properties**: Name, Email, PhoneNumber
- **Relationships**: Has many Orders
- **Audit**: Full audit trail with CreatedBy, ModifiedBy, timestamps

### Product
- **Properties**: Name, Description, Price, StockQuantity, SKU
- **Relationships**: Has many OrderItems
- **Audit**: Full audit trail with version tracking

### Order
- **Properties**: OrderNumber, OrderDate, TotalAmount, Status
- **Relationships**: Belongs to Customer, has many OrderItems
- **Audit**: Tracks who created and modified orders

### OrderItem
- **Properties**: Quantity, UnitPrice, TotalPrice (calculated)
- **Relationships**: Belongs to Order and Product
- **Audit**: Full audit trail

## Key Features Demonstrated

### 1. Automatic Audit Tracking
All entities automatically track:
- `CreatedBy` - User who created the entity
- `CreatedOn` - Timestamp of creation
- `ModifiedBy` - User who last modified the entity
- `ModifiedOn` - Timestamp of last modification
- `Version` - Optimistic concurrency version number

### 2. Soft Delete
Entities are marked as deleted rather than physically removed:
```csharp
await orderService.DeleteOrderAsync(orderId, userId);
// Order.IsDeleted = true, but record remains in database
```

### 3. DbContext Factory Pattern
Clean context creation with user context:
```csharp
using var context = _contextFactory.Create().WithUserId(userId);
```

### 4. Extension Methods
Convenient query helpers:
```csharp
var customer = await context.Customers.FindById(customerId);
```

### 5. In-Memory Database
Perfect for testing and demonstrations without external dependencies.

## Running the Application

```bash
cd src/Bounteous.Data.Sample
dotnet run
```

The application will:
1. Initialize logging with Serilog
2. Register all services via `AppStartup`
3. Create sample customers, products, and orders
4. Demonstrate CRUD operations with audit tracking
5. Show soft delete functionality
6. Display audit information (CreatedBy, ModifiedBy, etc.)

## Sample Output

The application logs all operations including:
- Entity creation with audit fields
- Order processing with line items
- Price updates with version tracking
- Soft delete operations
- Complete audit trail information

## Configuration

The application uses `appsettings.json` for configuration, though the in-memory database doesn't require a connection string.

## Dependencies

- **Bounteous.Core**: IoC container and configuration
- **Bounteous.Data**: Data access layer with audit support
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database provider
- **Serilog**: Structured logging

## Learning Points

This sample demonstrates:
1. How to set up a DbContext with Bounteous.Data
2. How to create domain entities with audit support
3. How to implement the factory pattern for DbContext
4. How to use the `IAppStartup` pattern for IoC registration
5. How to implement services that use the data layer
6. How audit fields are automatically populated
7. How soft delete works in practice
8. How to track user context throughout operations

## Next Steps

To adapt this for your own application:
1. Replace the in-memory provider with your database (SQL Server, PostgreSQL, etc.)
2. Update `SampleDbContextFactory.ApplyOptions()` with your database configuration
3. Create your own domain entities inheriting from `AuditBase`
4. Implement your business services
5. Configure your connection string in `appsettings.json`
