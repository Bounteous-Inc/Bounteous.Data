using Bounteous.Data.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain.Entities;

public class Category : AuditBase<long, Guid>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; }
}
