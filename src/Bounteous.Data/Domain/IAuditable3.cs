namespace Bounteous.Data.Domain;

public interface IAuditable
{
    public Guid Id { get; set; }
    DateTime CreatedOn { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime ModifiedOn { get; set; }
    DateTime SynchronizedOn { get; set; }
    Guid? ModifiedBy { get; set; }
    int Version { get; set; }
    bool IsDeleted { get; set; }
}