using Bounteous.Core.Extensions;
using Bounteous.Data.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bounteous.Data.Converters;

public class EnumConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
{
    public EnumConverter() 
        : base(
            v => v.GetDescription(), 
            v => v.ToEnum<TEnum>())
    {
    }
}