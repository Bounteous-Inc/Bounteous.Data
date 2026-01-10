using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Tests.Domain;

public class ReadOnlyLegacyProduct : ReadOnlyEntityBase<long>
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public string Category { get; set; } = string.Empty;
}
