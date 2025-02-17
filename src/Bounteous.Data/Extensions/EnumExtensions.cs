using System.ComponentModel;

namespace Bounteous.Data.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<T>(this T value) where T : IConvertible
    {
        if (!typeof(T).IsEnum) throw new ArgumentException("{0} must be an enumerated type", typeof(T).Name);

        var fi = value.GetType().GetField(value.ToString() ?? string.Empty);
        var attributes = (DescriptionAttribute[])fi!.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : value.ToString()!;
    }
    
    public static T FromDescription<T>(this string value) where T : Enum
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

        var enumValue = Enum
            .GetValues(typeof(T))
            .Cast<T>()
            .FirstOrDefault(x => x.GetDescription() == value);

        return enumValue!;
    }
    
}