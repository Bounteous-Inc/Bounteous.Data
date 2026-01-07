using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class LegacyCategory : IEntity<long>
{
    public long Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}
