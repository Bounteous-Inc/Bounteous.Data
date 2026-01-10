using System.ComponentModel;

namespace Bounteous.Data.Sample.Domain.Enums;

public enum ProductStatus
{
    [Description("Active")]
    Active = 1,
    
    [Description("Discontinued")]
    Discontinued = 2,
    
    [Description("Out of Stock")]
    OutOfStock = 3,
    
    [Description("Coming Soon")]
    ComingSoon = 4
}
