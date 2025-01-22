namespace Bounteous.DotNet.Data.Domain;

public interface IAuditable
{
    public Guid Id { get; set; }
    DateTime CreatedOn { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime ModifiedOn { get; set; }
    Guid? ModifiedBy { get; set; }
    public int Version { get; set; }
}