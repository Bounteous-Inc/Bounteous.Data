namespace Bounteous.Data.Domain;

public interface IAuditableMarker<TUserId>
    where TUserId : struct
{
    DateTime CreatedOn { get; set; }
    TUserId? CreatedBy { get; set; }
    DateTime ModifiedOn { get; set; }
    DateTime SynchronizedOn { get; set; }
    TUserId? ModifiedBy { get; set; }
    int Version { get; set; }
    bool IsDeleted { get; set; }
}
