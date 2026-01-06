namespace Bounteous.Data.Domain;

public abstract class AuditBase : AuditBase<Guid>
{
    public AuditBase() => Id = Guid.NewGuid();
}