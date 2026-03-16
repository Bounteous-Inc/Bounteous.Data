using System.ComponentModel.DataAnnotations;
using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class TempLog : AuditBase<Guid, long>, IHardDelete
{
    public TempLog() => Id = Guid.NewGuid();
    
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
    
    public DateTime LoggedAt { get; set; }
}
