using System.Globalization;
using Riok.Mapperly.Abstractions;

namespace Common.Application.Mappers;

[Mapper]
public static partial class StringToBuiltinsMapper
{
    public static DateOnly MapToDateOnly(this string value)
    {
        return DateOnly.ParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture
        );
    }
    
    public static DateTimeOffset MapToDateTimeOffset(this string value)
    {
        return DateTimeOffset.ParseExact(
            value,
            "yyyy-MM-ddTHH:mm:ss",
            CultureInfo.InvariantCulture
        );
    }
}
