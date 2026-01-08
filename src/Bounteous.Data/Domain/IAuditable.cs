namespace Bounteous.Data.Domain;

public interface IAuditable : IAuditable<Guid, Guid>;

public interface IAuditable<TId, TUserId> : IEntity<TId>, IAuditableMarker<TUserId>
    where TUserId : struct;