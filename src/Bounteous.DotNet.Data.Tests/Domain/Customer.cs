using System.ComponentModel.DataAnnotations;
using Bounteous.DotNet.Data.Domain;

namespace Bounteous.DotNet.Data.Tests.Domain;

public class Customer : AuditImmutableBase
{
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}