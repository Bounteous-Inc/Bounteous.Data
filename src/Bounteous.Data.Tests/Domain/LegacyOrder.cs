using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class LegacyOrder : AuditBase<int, Guid>
{
    public long CustomerId { get; set; }
    
    [MaxLength(100)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
}
