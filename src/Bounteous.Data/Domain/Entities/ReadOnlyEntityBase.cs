using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Domain.Entities;

/// <summary>
/// Base class for read-only entities.
/// Entities inheriting from this will be validated during SaveChanges
/// and cannot be modified through ReadOnlyDbSet.
/// </summary>
public abstract class ReadOnlyEntityBase<TId> : IReadOnlyEntity<TId>
{
    public TId Id { get; set; } = default!;
}
