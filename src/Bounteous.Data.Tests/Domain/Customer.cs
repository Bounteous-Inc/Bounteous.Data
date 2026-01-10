using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Tests.Domain;

public class Customer : AuditBase
{
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}