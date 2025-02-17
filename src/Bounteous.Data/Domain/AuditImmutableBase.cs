namespace Bounteous.Data.Domain;

public abstract class AuditImmutableBase : IAuditable, IDeleteable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
}