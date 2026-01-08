using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain;

public class Customer : AuditBase
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
