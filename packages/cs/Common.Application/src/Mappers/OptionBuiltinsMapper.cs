using Common.Utilities.Types;
using Riok.Mapperly.Abstractions;

namespace Common.Application.Mappers;

[Mapper]
public static partial class OptionBuiltinsMapper
{
    public static Option<long> MapToLongOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<long>();
        }

        var parsed = long.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<int> MapToIntOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<int>();
        }

        var parsed = int.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<short> MapToShortOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<short>();
        }

        var parsed = short.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<ulong> MapToULongOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<ulong>();
        }

        var parsed = ulong.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<uint> MapToUIntOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<uint>();
        }

        var parsed = uint.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<ushort> MapToUShortOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<ushort>();
        }

        var parsed = ushort.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<byte> MapToByteOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<byte>();
        }

        var parsed = byte.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<sbyte> MapToSByteOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<sbyte>();
        }

        var parsed = sbyte.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<float> MapToFloatOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<float>();
        }

        var parsed = float.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<double> MapToDoubleOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<double>();
        }

        var parsed = double.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<decimal> MapToDecimalOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<decimal>();
        }

        var parsed = decimal.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<T?> MapNullableToNullableOption<T>(this T? source)
    {
        return Option.Some(source);
    }

    public static Option<T> MapToOption<T>(this T source)
    {
        return Option.Some(source);
    }

    public static Option<Uri?> MapNullableToNullableUriOption(
        this string? source
    )
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<Uri?>();
        }

        var parsed = new Uri(source);
        return Option.Some<Uri?>(parsed);
    }

    public static Option<string?> MapNullableToNullableOption(
        this string? source
    )
    {
        return Option.Some(source);
    }

    public static Option<DateOnly?> MapNullableToNullableDateOnlyOption(
        this string? source
    )
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<DateOnly?>();
        }

        var parsed = DateOnly.Parse(source);
        return Option.Some<DateOnly?>(parsed);
    }

    public static Option<DateOnly> MapToDateOnlyOption(this string source)
    {
        return Option.Some(DateOnly.Parse(source));
    }

    public static Option<DateTime> MapToDateTimeOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<DateTime>();
        }

        var parsed = DateTime.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<TimeOnly> MapToTimeOnlyOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<TimeOnly>();
        }

        var parsed = TimeOnly.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<TimeSpan> MapToTimeSpanOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<TimeSpan>();
        }

        var parsed = TimeSpan.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<Guid> MapToGuidOption(this string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<Guid>();
        }

        var parsed = Guid.Parse(source);
        return Option.Some(parsed);
    }

    public static Option<DateTimeOffset> MapToDateTimeOffsetOption(
        this string? source
    )
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return Option.None<DateTimeOffset>();
        }

        var parsed = DateTimeOffset.Parse(source);
        return Option.Some(parsed);
    }
}
