using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class OrderItem : AuditBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public Guid OrderId { get; set; }
    
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;
    
    public Order? Order { get; set; }
}