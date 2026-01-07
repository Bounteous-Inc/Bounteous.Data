namespace Bounteous.Data.Domain;

public interface IAuditable<TId> : IEntity<TId>, IAuditableMarker
{
}
