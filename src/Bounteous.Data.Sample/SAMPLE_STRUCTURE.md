# Sample Application Domain Structure

This sample application demonstrates the recommended folder structure for organizing domain entities when using the Bounteous.Data library.

## Folder Structure

```
Domain/
├── Entities/          # Concrete entity classes
│   ├── Category.cs           - Inherits from AuditBase<long, Guid>
│   ├── Customer.cs           - Inherits from AuditBase
│   ├── Order.cs              - Inherits from AuditBase
│   ├── OrderItem.cs          - Inherits from AuditBase
│   ├── Product.cs            - Inherits from AuditBase
│   └── LegacySystem.cs       - Inherits from ReadOnlyEntityBase<int> (read-only)
└── Enums/             # Enumerations
    ├── OrderStatus.cs
    └── ProductStatus.cs
```

## Key Patterns Demonstrated

### 1. Regular Auditable Entities

Entities that need full audit tracking inherit from `AuditBase`:

```csharp
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Sample.Domain.Entities;

public class Customer : AuditBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

### 2. Auditable Entities with Custom ID Types

Entities with non-Guid IDs specify the ID and UserId types:

```csharp
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Sample.Domain.Entities;

public class Category : AuditBase<long, Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

### 3. Read-Only Entities

Entities from legacy systems or external sources that should not be modified:

```csharp
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Sample.Domain.Entities;

public class LegacySystem : ReadOnlyEntityBase<int>
{
    public string SystemName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime InstallDate { get; set; }
}
```

### 4. Enumerations

Domain enumerations are organized in a separate folder:

```csharp
using Bounteous.Data.Sample.Domain.Enums;

namespace Bounteous.Data.Sample.Domain.Entities;

public class Product : AuditBase
{
    public ProductStatus Status { get; set; } = ProductStatus.Active;
}
```

## Benefits of This Structure

1. **Clear Organization** - Entities and enums are logically separated
2. **Easy Navigation** - Developers can quickly find entity definitions
3. **Consistent with Library** - Mirrors the Bounteous.Data library structure
4. **Scalable** - Easy to add new entities or enums as the application grows
5. **Intent-Revealing** - Folder names clearly indicate their purpose

## Migration from Old Structure

If you have entities in the root `Domain/` folder, move them as follows:

- **Entity classes** → `Domain/Entities/`
- **Enums** → `Domain/Enums/`

Update namespaces:
- Old: `Bounteous.Data.Sample.Domain`
- New: `Bounteous.Data.Sample.Domain.Entities` or `Bounteous.Data.Sample.Domain.Enums`

Update using statements:
```csharp
// Old
using Bounteous.Data.Domain;

// New
using Bounteous.Data.Domain.Entities;
```
