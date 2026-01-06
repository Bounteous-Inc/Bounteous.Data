namespace Bounteous.Data.Domain;

/// <summary>
/// Base class for read-only entities that cannot be created, updated, or deleted.
/// Use this for legacy tables that should only be queried.
/// </summary>
public abstract class ReadOnlyEntityBase<TId> : IReadOnlyEntity<TId>
{
    public TId Id { get; set; } = default!;
}
