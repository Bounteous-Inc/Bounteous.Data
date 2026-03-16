using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class CustomerWithLongUserId : AuditBase<Guid, long>, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public CustomerWithLongUserId() => Id = Guid.NewGuid();
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}
