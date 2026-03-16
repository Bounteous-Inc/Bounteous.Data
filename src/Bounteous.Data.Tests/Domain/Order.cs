using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class Order : AuditBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public Guid CustomerId { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
    
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}