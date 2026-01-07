using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bounteous.Data.Converters;

public sealed class DateTimeConverter() : ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));