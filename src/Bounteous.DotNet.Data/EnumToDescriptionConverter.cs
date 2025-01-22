using Bounteous.DotNet.Data.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bounteous.DotNet.Data;

public class EnumToDescriptionConverter<TEnum>() : 
    ValueConverter<TEnum, string>(v => v.GetDescription(), v => v.FromDescription<TEnum>()) where TEnum : Enum;