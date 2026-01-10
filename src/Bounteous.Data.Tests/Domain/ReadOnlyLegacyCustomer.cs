using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Tests.Domain;

public class ReadOnlyLegacyCustomer : ReadOnlyEntityBase<int>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
}
