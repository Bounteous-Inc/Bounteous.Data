namespace Bounteous.Data;

public sealed class ReadOnlyValidationScope : IDisposable
{
    private static readonly AsyncLocal<bool> SuppressValidation = new();
   
    public static bool IsSuppressed => SuppressValidation.Value;
    public ReadOnlyValidationScope() => SuppressValidation.Value = true;
    public void Dispose() => SuppressValidation.Value = false;
}
