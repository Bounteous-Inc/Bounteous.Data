using Bounteous.Data.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bounteous.Data.Converters;

public class EnumToDescriptionConverter<TEnum>() : 
    ValueConverter<TEnum, string>(v => v.GetDescription(), v => v.FromDescription<TEnum>()) 
    where TEnum : Enum;