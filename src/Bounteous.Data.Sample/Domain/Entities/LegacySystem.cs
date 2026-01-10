using Bounteous.Data.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain.Entities;

public class LegacySystem : ReadOnlyEntityBase<int>
{
    [MaxLength(100)]
    public string SystemName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;
    
    public DateTime InstallDate { get; set; }
}
