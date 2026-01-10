using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Tests.Domain;

public class ProductWithLongUserId : AuditBase<Guid, long>
{
    public ProductWithLongUserId() => Id = Guid.NewGuid();
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
}
