using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain;

public class Category : AuditBase<long, Guid>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; }
}
