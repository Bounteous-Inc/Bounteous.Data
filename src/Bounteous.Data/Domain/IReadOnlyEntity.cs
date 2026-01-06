namespace Bounteous.Data.Domain;

/// <summary>
/// Marker interface for entities that are read-only and should not be created, updated, or deleted.
/// The DbContext will throw InvalidOperationException if any CUD operations are attempted.
/// </summary>
public interface IReadOnlyEntity<TId> : IEntity<TId>
{
}
