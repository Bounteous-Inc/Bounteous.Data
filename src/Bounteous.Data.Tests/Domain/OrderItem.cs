using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class OrderItem : AuditBase
{
    public Guid OrderId { get; set; }
    
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;
    
    public Order? Order { get; set; }
}