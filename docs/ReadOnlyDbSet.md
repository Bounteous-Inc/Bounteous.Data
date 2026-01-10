# ReadOnlyDbSet<TEntity, TId>

## Overview

`ReadOnlyDbSet<TEntity, TId>` is a wrapper around EF Core's `DbSet<T>` that provides **immediate fail-fast validation** for read-only entities. Unlike the deferred validation in `DbContextBase.SaveChanges()`, this wrapper throws `ReadOnlyEntityException` **immediately** when write operations are attempted.

## Design

### Two-Layer Protection Strategy

The module now provides **two complementary layers** of read-only enforcement:

1. **Immediate validation** (ReadOnlyDbSet) - Throws exceptions at the point of Add/Remove/Update calls
2. **Deferred validation** (DbContextBase) - Validates during SaveChanges as a safety net

### Architecture

```
IReadOnlyEntity<TId>
    ↑
    |
ReadOnlyEntityBase<TId>
    ↑
    |
Your Entity (e.g., ReadOnlyLegacyProduct)
    ↓
DbSet<TEntity>
    ↓
ReadOnlyDbSet<TEntity, TId> (wrapper)
```

## Usage

### Basic Usage

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    // Option 1: Return ReadOnlyDbSet directly from property
    public ReadOnlyDbSet<ReadOnlyLegacyProduct, long> ReadOnlyProducts 
        => Set<ReadOnlyLegacyProduct>().AsReadOnly();
    
    // Option 2: Use extension method on-demand
    public DbSet<ReadOnlyLegacyProduct> ReadOnlyProductsSet { get; set; }
    
    public void SomeMethod()
    {
        var readOnlySet = ReadOnlyProductsSet.AsReadOnly();
        // Use readOnlySet for queries
    }
}
```

### Querying (Allowed)

```csharp
// All query operations work normally
var products = await context.ReadOnlyProducts.ToListAsync();

var filtered = await context.ReadOnlyProducts
    .Where(p => p.Category == "Electronics")
    .Where(p => p.Price > 100m)
    .ToListAsync();

var product = await context.ReadOnlyProducts.FindAsync(123L);

// LINQ queries work as expected
var query = context.ReadOnlyProducts
    .OrderBy(p => p.Name)
    .Skip(10)
    .Take(20);
```

### Write Operations (Blocked)

All write operations throw `ReadOnlyEntityException` **immediately**:

```csharp
var readOnlySet = context.Set<ReadOnlyLegacyProduct>().AsReadOnly();

// ❌ Throws ReadOnlyEntityException immediately
readOnlySet.Add(product);
await readOnlySet.AddAsync(product);
readOnlySet.AddRange(products);
await readOnlySet.AddRangeAsync(products);

// ❌ Throws ReadOnlyEntityException immediately
readOnlySet.Remove(product);
readOnlySet.RemoveRange(products);

// ❌ Throws ReadOnlyEntityException immediately
readOnlySet.Update(product);
readOnlySet.UpdateRange(products);

// ❌ Throws ReadOnlyEntityException immediately
readOnlySet.Attach(product);
readOnlySet.AttachRange(products);
```

## Implementation Details

### ReadOnlyDbSet<TEntity, TId>

The wrapper class:
- Implements `IQueryable<TEntity>` for full LINQ support
- Delegates all query operations to the underlying `DbSet<T>`
- Throws `ReadOnlyEntityException` for all write operations
- Supports both synchronous and asynchronous operations
- Works with any ID type (int, long, Guid, etc.)

### Key Methods

**Allowed Operations:**
- `Find()` / `FindAsync()` - Find entities by key
- `AsQueryable()` - Get IQueryable for LINQ queries
- `AsAsyncEnumerable()` - Get async enumerable
- All `IQueryable<T>` operations (Where, Select, OrderBy, etc.)

**Blocked Operations:**
- `Add()` / `AddAsync()` / `AddRange()` / `AddRangeAsync()`
- `Remove()` / `RemoveRange()`
- `Update()` / `UpdateRange()`
- `Attach()` / `AttachRange()`

## Benefits

### 1. Fail-Fast Behavior
Errors are caught immediately at the point of the invalid operation, not deferred until `SaveChanges()`.

### 2. Clear Intent
Using `ReadOnlyDbSet` makes it explicit in the code that the entity is read-only.

### 3. Better Developer Experience
IDE autocomplete shows only valid operations, reducing mistakes.

### 4. Defense in Depth
Works alongside the existing `DbContextBase.ValidateReadOnlyEntities()` as a second layer of protection.

## Comparison: ReadOnlyDbSet vs SaveChanges Validation

| Aspect | ReadOnlyDbSet | SaveChanges Validation |
|--------|---------------|------------------------|
| **When** | Immediate (at Add/Remove/Update call) | Deferred (at SaveChanges) |
| **Error Location** | Exact line of invalid operation | Inside SaveChanges |
| **Stack Trace** | Points to the problematic code | Points to SaveChanges |
| **Prevention** | Prevents tracking entirely | Validates tracked entities |
| **Use Case** | Proactive protection | Safety net |

## Example Entity

```csharp
public class ReadOnlyLegacyProduct : ReadOnlyEntityBase<long>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
```

## Testing

Comprehensive tests are provided in `ReadOnlyDbSetTests.cs` covering:
- Query operations (allowed)
- All write operations (blocked)
- Synchronous and asynchronous methods
- Different ID types (int, long)
- IQueryable implementation
- Comparison with regular DbSet

## Migration Guide

### Before (Deferred Validation Only)

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    public DbSet<ReadOnlyLegacyProduct> ReadOnlyProducts { get; set; }
}

// Usage - error only at SaveChanges
context.ReadOnlyProducts.Add(product); // No error yet
await context.SaveChangesAsync(); // ❌ Throws here
```

### After (Immediate Validation)

```csharp
public class MyDbContext : DbContextBase<Guid>
{
    public ReadOnlyDbSet<ReadOnlyLegacyProduct, long> ReadOnlyProducts 
        => Set<ReadOnlyLegacyProduct>().AsReadOnly();
}

// Usage - error immediately
context.ReadOnlyProducts.Add(product); // ❌ Throws immediately
```

## Best Practices

1. **Use ReadOnlyDbSet for properties** - Return `ReadOnlyDbSet<T, TId>` from DbContext properties for read-only entities
2. **Keep SaveChanges validation** - Don't remove the existing validation; it serves as a safety net
3. **Consistent naming** - Use clear names like `ReadOnlyProducts` to indicate read-only intent
4. **Document intent** - Add XML comments explaining why entities are read-only

## See Also

- `ReadOnlyEntityBase<TId>` - Base class for read-only entities
- `IReadOnlyEntity<TId>` - Marker interface
- `ReadOnlyEntityException` - Exception thrown for violations
- `DbContextBase.ValidateReadOnlyEntities()` - Deferred validation
