using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Sample.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain.Entities;

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
