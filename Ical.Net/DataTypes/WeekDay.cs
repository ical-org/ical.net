//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an RFC 5545 "BYDAY" value.
/// </summary>
public class WeekDay : EncodableDataType
{
    public virtual int? Offset { get; set; }

    public virtual DayOfWeek DayOfWeek { get; set; }

    public WeekDay()
    { }

    public WeekDay(DayOfWeek day) : this()
    {
        DayOfWeek = day;
    }

    public WeekDay(DayOfWeek day, int num) : this(day)
    {
        Offset = num;
    }

    public WeekDay(DayOfWeek day, FrequencyOccurrence type) : this(day, (int) type) { }

    [Obsolete("Use TryParse instead.")]
    public WeekDay(string value)
    {
        if (TryParse(value, out var other))
        {
            CopyFrom(other);
        }
        else
        {
            throw new ArgumentException($"Cannot convert '{value}' to a {nameof(WeekDay)} object.", nameof(value));
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not WeekDay weekday)
        {
            return false;
        }

        return weekday.Offset == Offset && weekday.DayOfWeek == DayOfWeek;
    }

    public override int GetHashCode() => HashCode.Combine(Offset, DayOfWeek);

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not WeekDay weekday) return;

        Offset = weekday.Offset;
        DayOfWeek = weekday.DayOfWeek;
    }

    public int CompareTo(object? obj)
    {
        var weekday = obj switch
        {
            string str => new WeekDay(str),
            WeekDay day => day,
            _ => throw new ArgumentException($"Must be of type 'string' or 'Weekday'", nameof(obj))
        };

        var compare = DayOfWeek.CompareTo(weekday.DayOfWeek);
        if (compare == 0)
        {
            compare = Comparer<int?>.Default.Compare(Offset, weekday.Offset);
        }
        return compare;
    }

    public override string ToString()
    {
        if (!TryFormat(DayOfWeek, out var dayOfWeek))
        {
            return string.Empty;
        }

        if (Offset is not { } offset)
        {
            return dayOfWeek;
        }

        return offset.ToString(CultureInfo.InvariantCulture) + dayOfWeek;
    }

    #region Text Parsing

    public static bool TryParse(
        ReadOnlySpan<char> value,
#if NET
        [NotNullWhen(true)]
#endif
        out WeekDay? weekDay)
    {
        if (value.Length == 0)
        {
            weekDay = default;
            return false;
        }

        // Determine sign
        var sign = value[0] == '-' ? -1 : 1;

        if (value[0] is '+' or '-')
        {
            value = value.Slice(1);
        }

        // Count offset
        var offsetLength = 0;
        while (offsetLength < value.Length
            && char.IsDigit(value[offsetLength])
            && ++offsetLength < 2) ;

        // Parse offset later
        var offsetStr = value.Slice(0, offsetLength);

        // Parse day of week
        var dayOfWeekStr = value.Slice(offsetLength);
        if (!TryGetDayOfWeek(dayOfWeekStr, out var dayOfWeek))
        {
            weekDay = default;
            return false;
        }

        // Parse offset if there is one
        if (offsetStr.Length > 0)
        {
            var offsetParseResult = int.TryParse(value.Slice(0, offsetLength)
#if !NET
                .ToString()
#endif
                , out var offset);

            if (!offsetParseResult)
            {
                weekDay = default;
                return false;
            }

            // Week day with offset
            weekDay = new WeekDay(dayOfWeek, sign * offset);
            return true;
        }

        // Week day without offset
        weekDay = new WeekDay(dayOfWeek);
        return true;
    }

    internal static bool TryGetDayOfWeek(ReadOnlySpan<char> value, out DayOfWeek dayOfWeek)
    {
        if (value.Length != 2)
        {
            dayOfWeek = default;
            return false;
        }

        Span<char> upperValue = stackalloc char[2];
        value.ToUpperInvariant(upperValue);

        dayOfWeek = upperValue switch
        {
            // Check Sunday after
            // "SU" => DayOfWeek.Sunday,
            "MO" => DayOfWeek.Monday,
            "TU" => DayOfWeek.Tuesday,
            "WE" => DayOfWeek.Wednesday,
            "TH" => DayOfWeek.Thursday,
            "FR" => DayOfWeek.Friday,
            "SA" => DayOfWeek.Saturday,
            // Produces Sunday for invalid input or "SU"
            _ => default
        };

        // Check for invalid input
        return dayOfWeek != default || upperValue is "SU";
    }

    internal static bool TryFormat(DayOfWeek value,
#if NET
        [NotNullWhen(true)]
#endif
    out string? dayOfWeek)
    {
        dayOfWeek = value switch
        {
            DayOfWeek.Sunday => "SU",
            DayOfWeek.Monday => "MO",
            DayOfWeek.Tuesday => "TU",
            DayOfWeek.Wednesday => "WE",
            DayOfWeek.Thursday => "TH",
            DayOfWeek.Friday => "FR",
            DayOfWeek.Saturday => "SA",
            _ => null
        };

        return dayOfWeek is not null;
    }


    #endregion
}
