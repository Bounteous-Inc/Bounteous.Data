# Generic User ID Support - Implementation Guide

## Overview

The `DbContextBase` supports generic user ID types (`Guid`, `long`, `int`, etc.) through a type-safe generic implementation. This allows consumers to use different user ID types based on their authentication system requirements.

## Architecture

### Core Components

The library uses a **single generic implementation** for maximum flexibility:

1. **`IAuditableMarker<TUserId>`** - Generic audit marker interface (only version)
2. **`IAuditVisitor<TUserId>`** and **`AuditVisitor<TUserId>`** - Generic audit visitor pattern (only version)
3. **`DbContextBase<TUserId>`** - Generic base context class
4. **`IAuditable<TId, TUserId>`** - Generic auditable entity interface (dual-generic only)
5. **`AuditBase<TId, TUserId>`** - Generic audit base class (dual-generic only)

### Convenience Wrappers

For Guid-based systems (the most common scenario), convenience wrappers are provided:
- **`DbContextBase`** → inherits from `DbContextBase<Guid>`
- **`AuditBase`** → inherits from `AuditBase<Guid, Guid>` with auto-generated Guid ID
- **`IAuditable`** → inherits from `IAuditable<Guid, Guid>`

**Note:** All single-generic and non-generic redundant classes have been removed to eliminate code duplication. Only the flexible dual-generic versions remain, with thin convenience wrappers for the common Guid scenario.

## Usage Examples

### For Guid-Based Systems (Existing Behavior)

```csharp
// Option 1: Use the convenience wrapper (most common)
public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }
    
    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Register your models
    }
}

// Option 2: Use AuditBase for entities with Guid ID and Guid UserId
public class Product : AuditBase
{
    public string Name { get; set; }
    // Id, audit properties automatically included
}

// Option 3: Use IAuditable<TId, Guid> for custom ID type with Guid UserId
public class LegacyProduct : AuditBase<int, Guid>
{
    public string Name { get; set; }
}

// Usage
var context = new MyDbContext(options, observer);
context.WithUserId(Guid.Parse("..."));
await context.SaveChangesAsync();
```

### For Long-Based Systems (New)

```csharp
// Use the generic version with long
public class MyLongDbContext : DbContextBase<long>
{
    public MyLongDbContext(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }
    
    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Register your models
    }
}

// Entity definition with long user IDs using AuditBase
public class Product : AuditBase<Guid, long>
{
    public string Name { get; set; }
    // Id and audit properties automatically included with long UserId type
}

// Usage
var context = new MyLongDbContext(options, observer);
context.WithUserId(12345L);
await context.SaveChangesAsync();
```

### For Int-Based Systems

```csharp
public class MyIntDbContext : DbContextBase<int>
{
    public MyIntDbContext(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer)
    {
    }
    
    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Register your models
    }
}

// Entity definition with int user IDs using AuditBase
public class Product : AuditBase<Guid, int>
{
    public string Name { get; set; }
    // Id and audit properties automatically included with int UserId type
}

// Usage
var context = new MyIntDbContext(options, observer);
context.WithUserId(42);
await context.SaveChangesAsync();
```

## Migration Strategy

### For Existing Projects (Guid-based)

**Minimal changes required** for existing Guid-based projects:

```csharp
// DbContextBase still works (it's a convenience wrapper)
public class ExistingContext : DbContextBase { ... }

// Update single-generic to dual-generic:
// Before: IAuditable<int>
// After:  IAuditable<int, Guid> or AuditBase<int, Guid>
public class ExistingEntity : AuditBase<int, Guid> { ... }
```

### For New Projects or New Contexts

Choose the appropriate generic type based on your authentication system:

1. **Identity Server with Guid** → Use `DbContextBase<Guid>` or `DbContextBase`
2. **Legacy system with long** → Use `DbContextBase<long>`
3. **Simple int-based auth** → Use `DbContextBase<int>`

## Type Constraints

The generic type parameter `TUserId` must be a `struct` (value type):
- ✅ `Guid`, `long`, `int`, `uint`, `short`, etc.
- ❌ `string`, `object`, reference types

This ensures nullable semantics work correctly (`TUserId?`) and prevents boxing overhead.

## Key Benefits

1. **Type Safety** - Compile-time guarantees prevent type mismatches
2. **Performance** - No boxing/unboxing or runtime type checks
3. **Flexibility** - Easy to support any struct-based user ID type
4. **Backward Compatible** - Existing code requires no changes
5. **Clean Architecture** - Clear separation of concerns

## Architecture Files

### Core Generic Implementations (Single Source of Truth)
- `Domain/IAuditableMarkerGeneric.cs` - Generic audit marker interface
- `Audit/IAuditVisitorGeneric.cs` - Generic audit visitor (only version)
- `DbContextBaseGeneric.cs` - Generic DbContext base class
- `Domain/AuditBaseGeneric.cs` - Generic audit base class with dual-generic only
- `Domain/IAuditableGeneric.cs` - Generic IAuditable interface with dual-generic only

### Convenience Wrappers (Guid-based)
- `DbContextBase.cs` - Inherits from `DbContextBase<Guid>`
- `Domain/AuditBase.cs` - Inherits from `AuditBase<Guid, Guid>`
- `Domain/IAuditable.cs` - Inherits from `IAuditable<Guid, Guid>`

### Removed (Redundant Code Eliminated)
- ~~`Domain/IAuditableMarker.cs`~~ - Non-generic version removed
- ~~`Audit/IAuditVisitor.cs`~~ - Non-generic version removed
- ~~Single-generic `AuditBase<TId>`~~ - Removed, use `AuditBase<TId, Guid>` instead
- ~~Single-generic `IAuditable<TId>`~~ - Removed, use `IAuditable<TId, Guid>` instead

## Notes

- The generic constraint `where TUserId : struct` ensures only value types can be used
- All audit operations (Created, Modified, Deleted) automatically use the specified user ID type
- The `WithUserId()` method is type-safe and will only accept the correct user ID type
