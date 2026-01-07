using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class Order : AuditBase
{
    public Guid CustomerId { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}