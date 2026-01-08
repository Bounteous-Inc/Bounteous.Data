using Bounteous.Data.Domain;
using System.ComponentModel.DataAnnotations;

namespace Bounteous.Data.Sample.Domain;

public class LegacySystem : ReadOnlyEntityBase<int>
{
    [MaxLength(100)]
    public string SystemName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;
    
    public DateTime InstallDate { get; set; }
}
