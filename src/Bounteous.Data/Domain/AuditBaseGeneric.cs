namespace Bounteous.Data.Domain;

public abstract class AuditBase<TId> : IAuditable<TId>, IDeleteable
{
    public TId Id { get; set; } = default!;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime SynchronizedOn { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}
