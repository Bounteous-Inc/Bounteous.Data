using Bounteous.Core.Time;
using Bounteous.Data.Domain;
using Bounteous.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Audit;

public interface IAuditVisitor
{
    void AcceptNew(EntityEntry entry, Guid? userId);
    void AcceptModified(EntityEntry entry, Guid? userId);
    void AcceptDeleted(EntityEntry entry, Guid? userId);
}

public class AuditVisitor : IAuditVisitor
{
    public void AcceptNew(EntityEntry entry, Guid? userId)
    {
        if (entry.Entity is not IAuditable entityEntry) return;

        entityEntry.CreatedOn = Clock.Utc.Now;
        entityEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        
        entityEntry.ModifiedBy = userId.Value;
        if (entityEntry.CreatedBy.IsNullOrEmpty())
            entityEntry.CreatedBy = userId.Value;
        entityEntry.Version += 1;
    }

    public void AcceptModified(EntityEntry entry, Guid? userId)
    {
        if (entry.Entity is not IAuditable entityEntry) return;
        entityEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        entityEntry.ModifiedBy = userId;
        entityEntry.Version += 1;
    }

    public void AcceptDeleted(EntityEntry entry, Guid? userId)
    {
        if(entry.Entity is not IAuditable auditableEntry) return;
        
        ((IDeleteable) entry.Entity).IsDeleted = true;
        entry.State = EntityState.Modified;
        auditableEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        auditableEntry.ModifiedBy = userId.Value;
    }
}