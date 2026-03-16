using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class LegacyOrder : AuditBase<int, Guid>, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public long CustomerId { get; set; }
    
    [MaxLength(100)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
}
