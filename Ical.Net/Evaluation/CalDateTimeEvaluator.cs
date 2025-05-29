//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;

namespace Ical.Net.Evaluation;

/// <summary>
/// Provides evaluation and utility methods for working with <see cref="CalDateTime"/>
/// and <see cref="CalDateTimeZoned"/> objects,  including conversions between timezones,
/// adding durations, and formatting.
/// </summary>
public static class CalDateTimeEvaluator
{
    private static ZonedDateTime? ResolveZonedDateTime(CalDateTime calDateTime)
    {
        if (calDateTime.IsUtc)
        {
            return LocalDateTime.FromDateTime(calDateTime.Value).InZoneStrictly(DateTimeZone.Utc);
        }
        if (calDateTime.IsFloating)
        {
            return null;
        }
        var zone = DateUtil.GetZone(calDateTime.TzId!);
        return DateUtil.ToZonedDateTimeLeniently(calDateTime.Value, zone.Id);
    }

    public static CalDateTimeZoned AsZoned(this CalDateTime calDateTime)
    {
        var zonedDateTime = ResolveZonedDateTime(calDateTime);
        return new CalDateTimeZoned(calDateTime, zonedDateTime);
    }

    /// <summary>
    /// Converts the <see cref="CalDateTime.Value"/> to a date/time
    /// within the specified <see paramref="otherTzId"/> timezone.
    /// <para/>
    /// If <see cref="CalDateTime.IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="CalDateTime.Value"/> is considered as local time for every timezone:
    /// The returned <see cref="CalDateTime.Value"/> is unchanged and the <see paramref="otherTzId"/> is set as <see cref="TzId"/>.
    /// </summary>
    /// <param name="zoned">The CalDateTimeZoned instance.</param>
    /// <param name="otherTzId">The target time zone ID, or null for floating.</param>
    /// <returns>A new CalDateTime in the specified time zone.</returns>
    public static CalDateTimeZoned ToTimeZone(this CalDateTimeZoned zoned, string? otherTzId)
    {
        if (otherTzId is null)
            return new CalDateTimeZoned(new CalDateTime(zoned.CalDateTime.Value, null, zoned.CalDateTime.HasTime), null);

        ZonedDateTime converted;
        if (zoned.IsFloating)
        {
            // Make sure, we properly fix the time if it doesn't exist in the target tz.
            converted = DateUtil.ToZonedDateTimeLeniently(zoned.Value, otherTzId);
        }
        else
        {
            converted = otherTzId != zoned.TzId
                ? zoned.ZonedDateTime!.Value.WithZone(DateUtil.GetZone(otherTzId))
                : zoned.ZonedDateTime!.Value;
        }
        var convCalDt = new CalDateTime(converted.ToDateTimeUnspecified(), otherTzId, zoned.CalDateTime.HasTime);
        
        return new CalDateTimeZoned(convCalDt, converted);
    }

    /// <summary>
    /// Converts the specified <see cref="CalDateTimeZoned"/> instance to UTC timezone.
    /// </summary>
    /// <param name="zoned">The <see cref="CalDateTimeZoned"/> instance to convert.</param>
    /// <returns>A new <see cref="CalDateTimeZoned"/> instance representing the same date and time in UTC.</returns>
    public static CalDateTimeZoned ToUtc(this CalDateTimeZoned zoned)
        => ToTimeZone(zoned, CalDateTime.UtcTzId);

    /// <summary>
    /// Add the specified <see cref="DataTypes.Duration"/> to this instance/>.
    /// </summary>
    /// <remarks>
    /// In correspondence to RFC5545, the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTimeZoned Add(this CalDateTimeZoned zoned, DataTypes.Duration d)
    {
        if (!zoned.HasTime && d.HasTime)
        {
            throw new InvalidOperationException($"This instance represents a 'date-only' value '{zoned.ToString()}'. Only multiples of full days can be added to a 'date-only' instance, not '{d}'");
        }

        // RFC 5545 3.3.6:
        // If the property permits, multiple "duration" values are specified by a COMMA-separated
        // list of values.The format is based on the[ISO.8601.2004] complete representation basic
        // format with designators for the duration of time.The format can represent nominal
        // durations(weeks and days) and accurate durations(hours, minutes, and seconds).
        // Note that unlike[ISO.8601.2004], this value type doesn't support the "Y" and "M"
        // designators to specify durations in terms of years and months.
        //
        // The duration of a week or a day depends on its position in the calendar. In the case
        // of discontinuities in the timescale, such as the change from standard time to daylight
        // time and back, the computation of the exact duration requires the subtraction or
        // addition of the change of duration of the discontinuity.Leap seconds MUST NOT be
        // considered when computing an exact duration.When computing an exact duration, the
        // greatest order time components MUST be added first, that is, the number of days MUST be
        // added first, followed by the number of hours, number of minutes, and number of seconds.

        (TimeSpan? nominalPart, TimeSpan? exactPart) dt;
        if (zoned.TzId is null)
            dt = (d.ToTimeSpanUnspecified(), null);
        else
            dt = (d.HasDate ? d.DateAsTimeSpan : null, d.HasTime ? d.TimeAsTimeSpan : null);

        var newDateTime = zoned.CalDateTime;
        var newZoned = zoned;
        if (dt.nominalPart is not null)
            newDateTime = new CalDateTime(newDateTime.Value.Add(dt.nominalPart.Value), zoned.TzId, zoned.HasTime);

        if (dt.exactPart is not null)
        {
            var utcZoned = new CalDateTimeZoned(newDateTime, ResolveZonedDateTime(newDateTime)).ToUtc();
            newDateTime = new CalDateTime(utcZoned.CalDateTime.Value.Add(dt.exactPart.Value), CalDateTime.UtcTzId, utcZoned.HasTime);
            utcZoned = new CalDateTimeZoned(newDateTime, ResolveZonedDateTime(newDateTime));
            if (zoned.TzId is not null)
                return utcZoned.ToTimeZone(zoned.TzId);
        }

        if (zoned.TzId is not null)
        {
            // Convert to the original timezone even if already set to ensure we're not in a non-existing time.
            newZoned = new CalDateTimeZoned(newDateTime, ResolveZonedDateTime(newDateTime)).ToTimeZone(zoned.TzId);
            newDateTime = newZoned.CalDateTime;
        }

        return new CalDateTimeZoned(newDateTime, newZoned.ZonedDateTime);
    }

    /// <summary>Returns a new <see cref="TimeSpan" /> from subtracting the specified <see cref="CalDateTime"/> from to the value of this instance.</summary>
    public static TimeSpan SubtractExact(this CalDateTimeZoned zoned, CalDateTimeZoned other)
        => zoned.ToTimeZone(CalDateTime.UtcTzId).Utc!.Value - other.ToTimeZone(CalDateTime.UtcTzId).Utc!.Value;

    /// <summary>
    /// Returns a new <see cref="DataTypes.Duration"/> from
    /// subtracting the specified <see cref="CalDateTime"/>
    /// from the value of this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static DataTypes.Duration Subtract(this CalDateTimeZoned zoned, CalDateTimeZoned other)
    {
        if (zoned.HasZone)
            return SubtractExact(zoned, other).ToDurationExact();

        if (zoned.HasTime != other.HasTime)
            throw new InvalidOperationException($"Trying to calculate the difference between dates of different types. An instance of type DATE cannot be subtracted from a DATE-TIME and vice versa: {zoned.ToString()} - {other.ToString()}");

        return (zoned.Value - other.Value).ToDuration();
    }

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of years to the value of this instance.
    /// </summary>
    public static CalDateTimeZoned AddYears(this CalDateTimeZoned zoned, int years)
        => new CalDateTime(zoned.CalDateTime.Value.AddYears(years), zoned.TzId, zoned.HasTime).AsZoned();

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of months to the value of this instance.
    /// </summary>
    public static CalDateTimeZoned AddMonths(this CalDateTimeZoned zoned, int months)
        => new CalDateTime(zoned.CalDateTime.Value.AddMonths(months), zoned.TzId, zoned.HasTime).AsZoned();

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of days to the value of this instance.
    /// </summary>
    public static CalDateTimeZoned AddDays(this CalDateTimeZoned zoned, int days)
        => new CalDateTime(zoned.CalDateTime.Value.AddDays(days), zoned.TzId, zoned.HasTime).AsZoned();

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of hours to the value of this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws when attempting to add a time span to a date-only instance.
    /// </exception>
    public static CalDateTimeZoned AddHours(this CalDateTimeZoned zoned, int hours)
        => zoned.Add(DataTypes.Duration.FromHours(hours));

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of minutes to the value of this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTimeZoned AddMinutes(this CalDateTimeZoned zoned, int minutes)
        => zoned.Add(DataTypes.Duration.FromMinutes(minutes));

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of seconds to the value of this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTimeZoned AddSeconds(this CalDateTimeZoned zoned, int seconds)
        => zoned.Add(DataTypes.Duration.FromSeconds(seconds));

    /// <summary>
    /// Returns a string that includes the date and time with the timezone identifier,
    /// formatted according to the specified format and culture information.
    /// </summary>
    public static string ToString(this CalDateTimeZoned zoned, string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        var dateTimeOffset =
            ResolveZonedDateTime(zoned.CalDateTime)?.ToDateTimeOffset()
            ?? DateUtil.ToZonedDateTimeLeniently(zoned.Value, zoned.TzId ?? string.Empty).ToDateTimeOffset();

        // Use the .NET format options to format the DateTimeOffset
        var tzIdString = zoned.TzId is not null ? $" {zoned.TzId}" : string.Empty;

        return zoned.HasTime
            ? $"{dateTimeOffset.ToString(format, formatProvider)}{tzIdString}"
            : $"{DateOnly.FromDateTime(dateTimeOffset.Date).ToString(format ?? "d", formatProvider)}{tzIdString}";
    }
}
