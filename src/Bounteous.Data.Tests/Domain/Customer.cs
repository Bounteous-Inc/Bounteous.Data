using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class Customer : AuditBase, ISoftDelete
{
    public bool IsDeleted { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}