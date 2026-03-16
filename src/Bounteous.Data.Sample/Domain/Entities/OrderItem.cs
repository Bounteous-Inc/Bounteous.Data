using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Sample.Domain.Entities;

public class OrderItem : AuditBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public Guid OrderId { get; set; }
    
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public Order Order { get; set; } = null!;
    
    public Product Product { get; set; } = null!;
}
