using System.Globalization;
using Riok.Mapperly.Abstractions;

namespace Common.Application.Mappers;

[Mapper]
public static partial class BuiltinsToStringMapper
{
    public static string MapToString(DateOnly value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static string MapToString(DateTimeOffset value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static string MapToString(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static string MapToString(TimeOnly value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }
}