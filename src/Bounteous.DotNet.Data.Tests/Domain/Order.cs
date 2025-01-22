using System.ComponentModel.DataAnnotations;
using Bounteous.DotNet.Data.Domain;

namespace Bounteous.DotNet.Data.Tests.Domain;

public class Order : AuditImmutableBase
{
    public Guid CustomerId { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
}