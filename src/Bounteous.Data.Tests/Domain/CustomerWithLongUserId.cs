using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class CustomerWithLongUserId : AuditBase<Guid, long>
{
    public CustomerWithLongUserId() => Id = Guid.NewGuid();
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}
