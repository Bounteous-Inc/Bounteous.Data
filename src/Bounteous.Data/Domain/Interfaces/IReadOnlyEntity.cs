namespace Bounteous.Data.Domain.Interfaces;

/// <summary>
/// Marker interface for read-only entities.
/// Entities implementing this interface will be validated during SaveChanges
/// and cannot be modified through ReadOnlyDbSet.
/// </summary>
public interface IReadOnlyEntity<TId> : IEntity<TId>
{
}
