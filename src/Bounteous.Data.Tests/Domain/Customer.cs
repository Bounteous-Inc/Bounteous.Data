using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain;

namespace Bounteous.Data.Tests.Domain;

public class Customer : AuditImmutableBase
{
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}