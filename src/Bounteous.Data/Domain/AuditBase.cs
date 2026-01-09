namespace Bounteous.Data.Domain;

public abstract class AuditBase : AuditBase<Guid, Guid>
{
    public AuditBase() => Id = Guid.NewGuid();
}

public abstract class AuditBase<TId, TUserId> : IAuditable<TId, TUserId>, IDeleteable
    where TUserId : struct
{
    public TId Id { get; set; } = default!;
    public TUserId CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime SynchronizedOn { get; set; }
    public TUserId ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}