using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Sample.Domain.Entities;

public class OrderItem : AuditBase
{
    public Guid OrderId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public Order Order { get; set; } = null!;
    
    public Product Product { get; set; } = null!;
}
