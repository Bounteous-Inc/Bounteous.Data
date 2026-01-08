using Bounteous.Core.Time;
using Bounteous.Data.Domain;
using Bounteous.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Audit;

public interface IAuditVisitor<TUserId>
    where TUserId : struct
{
    void AcceptNew(EntityEntry entry, TUserId? userId);
    void AcceptModified(EntityEntry entry, TUserId? userId);
    void AcceptDeleted(EntityEntry entry, TUserId? userId);
}

public class AuditVisitor<TUserId> : IAuditVisitor<TUserId>
    where TUserId : struct
{
    public void AcceptNew(EntityEntry entry, TUserId? userId)
    {
        if (entry.Entity is not IAuditableMarker<TUserId> entityEntry) return;

        entityEntry.CreatedOn = Clock.Utc.Now;
        entityEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        
        entityEntry.ModifiedBy = userId.Value;
        if (!entityEntry.CreatedBy.HasValue)
            entityEntry.CreatedBy = userId.Value;
        entityEntry.Version += 1;
    }

    public void AcceptModified(EntityEntry entry, TUserId? userId)
    {
        if (entry.Entity is not IAuditableMarker<TUserId> entityEntry) return;
        entityEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        entityEntry.ModifiedBy = userId;
        entityEntry.Version += 1;
    }

    public void AcceptDeleted(EntityEntry entry, TUserId? userId)
    {
        if(entry.Entity is not IAuditableMarker<TUserId> auditableEntry) return;
        
        ((IDeleteable) entry.Entity).IsDeleted = true;
        entry.State = EntityState.Modified;
        auditableEntry.ModifiedOn = Clock.Utc.Now;

        if (!userId.HasValue) return;
        auditableEntry.ModifiedBy = userId.Value;
    }
}
