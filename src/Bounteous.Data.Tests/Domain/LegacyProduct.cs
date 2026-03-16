using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class LegacyProduct : AuditBase<long, Guid>, ISoftDelete
{
    public bool IsDeleted { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
}
