# ReadOnlyDbSet Implementation Summary

## Overview

This document summarizes the lean `ReadOnlyDbSet<TEntity, TId>` implementation that provides fail-fast validation for read-only entities while minimizing coupling to Entity Framework Core internals.

## Design Decisions

### Why Not Inherit from DbSet?

**`DbSet<TEntity>` cannot be inherited** because:
1. No public constructors (only internal ones)
2. Methods are not virtual (cannot be overridden)
3. Tightly coupled to EF Core infrastructure

### Why Not Use Keyless Entity Types?

**Keyless Entity Types are not suitable** because:
1. They have no primary key (cannot use `Find(id)`)
2. Only useful for views/reports that return collections
3. Your entities HAVE keys and need to be queryable by ID

### The Lean Solution: Composition + Implicit Conversion

The implementation uses:
1. **Composition**: Wraps a `DbSet<TEntity>`
2. **Implicit conversion**: Converts to `DbSet<TEntity>` for queries
3. **Explicit methods**: Only for mutating operations that throw exceptions

## Implementation

### ReadOnlyDbSet.cs (~75 lines)

```csharp
public class ReadOnlyDbSet<TEntity, TId> where TEntity : class, IReadOnlyEntity<TId>
{
    private readonly DbSet<TEntity> innerDbSet;
    private readonly string entityTypeName;

    public ReadOnlyDbSet(DbSet<TEntity> dbSet)
    {
        innerDbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        entityTypeName = typeof(TEntity).Name;
    }

    /// <summary>
    /// Implicit conversion to DbSet for query operations.
    /// This allows full LINQ support including async operations.
    /// </summary>
    public static implicit operator DbSet<TEntity>(ReadOnlyDbSet<TEntity, TId> readOnlySet)
        => readOnlySet.innerDbSet;

    // Mutating operations - all throw immediately
    public EntityEntry<TEntity> Add(TEntity entity)
        => throw new ReadOnlyEntityException(entityTypeName, "create");
    
    // ... other mutating methods
}
```

### Usage in DbContext

```csharp
public class ElevationsDbContext : DbContextBase<long>
{
    // Read-only entities
    public ReadOnlyDbSet<Company, long> Companies => Set<Company>().AsReadOnly<Company, long>();
    public ReadOnlyDbSet<User, long> Users => Set<User>().AsReadOnly<User, long>();
    
    // Writable entities
    public DbSet<Elevation> Elevations { get; set; }
    public DbSet<Frame> Frames { get; set; }
}
```

### Usage in Application Code

```csharp
// Queries work perfectly - implicit conversion to DbSet<T>
var companies = await context.Companies.ToListAsync();
var company = await context.Companies.FindAsync(1L);
var filtered = await context.Companies
    .Where(c => c.Code.StartsWith("ACME"))
    .OrderBy(c => c.Name)
    .ToListAsync();

// Mutations fail immediately - fail fast!
context.Companies.Add(new Company()); // ❌ Throws ReadOnlyEntityException
```

## Benefits

✅ **Minimal EF Core coupling** - Only depends on `DbSet<T>` and `EntityEntry<T>`  
✅ **All queries work** - Implicit conversion gives full `DbSet<T>` functionality  
✅ **Fail-fast on mutations** - Immediate exceptions with clear messages  
✅ **Upgrade-safe** - No internal EF Core APIs used  
✅ **Clean & lean** - ~75 lines of code  
✅ **Full async support** - All EF Core async methods work via implicit conversion  
✅ **Primary key support** - `Find(id)` and `FindAsync(id)` work perfectly  

## Key Files

- `src/Bounteous.Data/Domain/ReadOnlyDbSet.cs` - Main implementation
- `src/Bounteous.Data/Domain/ReadOnlyDbSetExtensions.cs` - Extension method
- `src/Bounteous.Data/Domain/IReadOnlyEntity.cs` - Marker interface
- `src/Bounteous.Data/Domain/ReadOnlyEntityBase.cs` - Base class
- `src/Bounteous.Data/Exceptions/ReadOnlyEntityException.cs` - Custom exception
- `src/Bounteous.Data/DbContextBase.cs` - Contains `ValidateReadOnlyEntities()` as safety net

## Two Layers of Protection

1. **Immediate (Fail-Fast)**: `ReadOnlyDbSet` throws on Add/Update/Delete
2. **Deferred (Safety Net)**: `DbContextBase.SaveChanges()` validates read-only entities

This dual approach ensures read-only entities are protected even if someone bypasses the `ReadOnlyDbSet`.
