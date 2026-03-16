using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain.Entities;

public class Customer : AuditBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
