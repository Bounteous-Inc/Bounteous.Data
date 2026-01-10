# Domain Folder Reorganization

## New Structure

The `Domain` folder has been reorganized into three logical subfolders for better clarity:

```
Domain/
├── Entities/          # Base entity classes
│   ├── AuditBase.cs
│   └── ReadOnlyEntityBase.cs
├── Interfaces/        # Core interfaces
│   ├── IAuditable.cs
│   ├── IAuditableMarker.cs
│   ├── IDeleteable.cs
│   ├── IEntity.cs
│   └── IReadOnlyEntity.cs
└── ReadOnly/          # Read-only infrastructure
    ├── ReadOnlyDbSet.cs
    └── ReadOnlyDbSetExtensions.cs
```

## Namespace Changes

### Old Namespaces
- `Bounteous.Data.Domain` (all files)

### New Namespaces
- `Bounteous.Data.Domain.Entities` - Base classes
- `Bounteous.Data.Domain.Interfaces` - Interfaces
- `Bounteous.Data.Domain.ReadOnly` - ReadOnlyDbSet infrastructure

## Migration Guide

### For Library Consumers

Update your using statements:

**Old:**
```csharp
using Bounteous.Data.Domain;
```

**New (choose what you need):**
```csharp
using Bounteous.Data.Domain.Entities;    // For AuditBase, ReadOnlyEntityBase
using Bounteous.Data.Domain.Interfaces;  // For IEntity, IAuditable, IReadOnlyEntity, etc.
using Bounteous.Data.Domain.ReadOnly;    // For ReadOnlyDbSet, AsReadOnly()
```

### Common Scenarios

**Scenario 1: Entity inherits from AuditBase**
```csharp
using Bounteous.Data.Domain.Entities;

public class MyEntity : AuditBase
{
    // ...
}
```

**Scenario 2: Entity inherits from ReadOnlyEntityBase**
```csharp
using Bounteous.Data.Domain.Entities;

public class MyReadOnlyEntity : ReadOnlyEntityBase<long>
{
    // ...
}
```

**Scenario 3: Using ReadOnlyDbSet in DbContext**
```csharp
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Domain.ReadOnly;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContextBase<Guid>
{
    public ReadOnlyDbSet<Company, long> Companies => Set<Company>().AsReadOnly<Company, long>();
}
```

**Scenario 4: Implementing interfaces directly**
```csharp
using Bounteous.Data.Domain.Interfaces;

public class MyEntity : IEntity<Guid>, IAuditable<Guid, Guid>
{
    // ...
}
```

## Benefits

1. **Clarity** - Intent-revealing folder names make it clear what each file does
2. **Organization** - Related files are grouped together
3. **Discoverability** - Easier to find what you need
4. **Maintainability** - Clear separation of concerns
5. **Industry Standard** - Follows common .NET project organization patterns
