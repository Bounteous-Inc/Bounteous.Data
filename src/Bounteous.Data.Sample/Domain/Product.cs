using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain;

public class Product : AuditBase
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; }
    
    [MaxLength(50)]
    public string? Sku { get; set; }
    
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    
    public DateTime? LastRestockedOn { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
