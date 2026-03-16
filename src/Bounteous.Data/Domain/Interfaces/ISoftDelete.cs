namespace Bounteous.Data.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
