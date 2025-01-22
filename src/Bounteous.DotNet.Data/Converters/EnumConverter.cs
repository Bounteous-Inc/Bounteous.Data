using Bounteous.DotNet.Data.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xerris.DotNet.Core.Extensions;

namespace Bounteous.DotNet.Data.Converters;

public class EnumConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
{
    public EnumConverter() 
        : base(
            v => v.GetDescription(), 
            v => v.ToEnum<TEnum>())
    {
    }
}