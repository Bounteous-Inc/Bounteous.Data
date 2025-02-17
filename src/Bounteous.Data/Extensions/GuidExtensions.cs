namespace Bounteous.Data.Extensions;

public static class GuidExtensions
{
    public static bool IsNullOrEmpty(this Guid? value)
        => value == null || value.Value == Guid.Empty;
}