using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class ProductWithIntUserId : AuditBase<Guid, int>, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public ProductWithIntUserId() => Id = Guid.NewGuid();
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
}
