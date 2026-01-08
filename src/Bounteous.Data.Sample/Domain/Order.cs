using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain;

public class Order : AuditBase
{
    public Guid CustomerId { get; set; }
    
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public OrderStatus Status { get; set; }
    
    public Customer Customer { get; set; } = null!;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
