//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;

namespace Ical.Net.Evaluation;

/// <summary>
/// This class belongs to the evaluation layer of the library.
/// It provides extension methods for the <see cref="CalDateTime"/> class.
/// </summary>
public static class CalDateTimeExtensions
{
    private sealed class ZonedDateTimeBox(ZonedDateTime value)
    {
        public ZonedDateTime Value { get; } = value;
    }

    /// <summary>
    /// The cache for storing resolved <see cref="ZonedDateTime"/> objects for <see cref="CalDateTime"/> objects
    /// This is a thread-safe dictionary that uses weak references to avoid memory leaks.
    /// Dead <see cref="CalDateTime"/> keys are automatically removed from the cache.
    /// Key and Value must be reference types, so we use <see cref="ZonedDateTimeBox"/> to box the value.
    /// <para/>
    /// If a <see cref="CalDateTime"/> is instantiated from a lenient timezone conversion, this leniently
    /// determined <see cref="CalDateTime"/> must not be converted again.
    /// Lenient conversions resolve non-existing or ambiguous times caused by DST transitions.
    /// <para/>
    /// Therefore, when converting from one timezone to another, the conversion is done with the
    /// <see cref="ZonedDateTime"/> object created at the time of (first) conversion,
    /// which is stored in the cache.
    /// </summary>
    private static readonly ConditionalWeakTable<CalDateTime, ZonedDateTimeBox> _cache = new();

    /// <summary>
    /// Stores a <see cref="ZonedDateTime"/> in the cache for the given <see cref="CalDateTime"/>.
    /// </summary>
    private static void AddToCache(CalDateTime calDateTime, ZonedDateTime zonedDateTime)
        => _cache.Add(calDateTime, new ZonedDateTimeBox(zonedDateTime));

    /// <summary>
    /// Resolves a <see cref="CalDateTime"/> to a <see cref="ZonedDateTime"/>,
    /// using the cache if possible.
    /// </summary>
    private static ZonedDateTime? ResolveZonedDateTime(CalDateTime calDateTime)
    {
        // Try to get from cache
        if (_cache.TryGetValue(calDateTime, out var zoned))
        {
            return zoned.Value;
        }

        // Compute and cache
        ZonedDateTime? result = null;
        if (calDateTime.IsUtc)
        {
            result = LocalDateTime.FromDateTime(calDateTime.Value).InZoneStrictly(DateTimeZone.Utc);
        }
        else if (!calDateTime.IsFloating)
        {
            var zone = DateUtil.GetZone(calDateTime.TzId!);
            result = DateUtil.ToZonedDateTimeLeniently(calDateTime.Value, zone.Id);
        }

        if (result.HasValue)
            AddToCache(calDateTime, result.Value);

        return result;
    }

    /// <summary>
    /// Converts the date/time to UTC (Coordinated Universal Time)
    /// If <see cref="CalDateTime.IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="CalDateTime.Value"/> is considered as local time for every timezone:
    /// The returned <see cref="CalDateTime.Value"/> is unchanged, but with <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    public static DateTime AsUtc(this CalDateTime calDateTime)
        => DateTime.SpecifyKind(ToTimeZone(calDateTime, CalDateTime.UtcTzId).Value, DateTimeKind.Utc);

    /// <summary>
    /// Converts the <see cref="CalDateTime.Value"/> to a date/time
    /// within the specified <see paramref="otherTzId"/> timezone.
    /// <para/>
    /// If <see cref="CalDateTime.IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="CalDateTime.Value"/> is considered as local time for every timezone:
    /// The returned <see cref="CalDateTime.Value"/> is unchanged and the <see paramref="otherTzId"/> is set as <see cref="TzId"/>.
    /// </summary>
    /// <param name="calDateTime">The CalDateTime instance.</param>
    /// <param name="otherTzId">The target time zone ID, or null for floating.</param>
    /// <returns>A new CalDateTime in the specified time zone.</returns>
    public static CalDateTime ToTimeZone(this CalDateTime calDateTime, string? otherTzId)
    {
        if (otherTzId is null)
            return new CalDateTime(calDateTime.Value, null, calDateTime.HasTime);

        var zoned = ResolveZonedDateTime(calDateTime);

        ZonedDateTime converted;
        if (calDateTime.IsFloating)
        {
            // Make sure, we properly fix the time if it doesn't exist in the target tz.
            converted = DateUtil.ToZonedDateTimeLeniently(calDateTime.Value, otherTzId);
        }
        else
        {
            converted = otherTzId != calDateTime.TzId
                ? zoned!.Value.WithZone(DateUtil.GetZone(otherTzId))
                : zoned!.Value;
        }
        var convCalDt = new CalDateTime(converted.ToDateTimeUnspecified(), otherTzId, calDateTime.HasTime);
        AddToCache(convCalDt, converted);
        return convCalDt;
    }

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
    public static CalDateTime Add(this CalDateTime calDateTime, DataTypes.Duration d)
    {
        if (!calDateTime.HasTime && d.HasTime)
        {
            throw new InvalidOperationException($"This instance represents a 'date-only' value '{calDateTime.ToString()}'. Only multiples of full days can be added to a 'date-only' instance, not '{d}'");
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
        // of discontinuities in the time scale, such as the change from standard time to daylight
        // time and back, the computation of the exact duration requires the subtraction or
        // addition of the change of duration of the discontinuity.Leap seconds MUST NOT be
        // considered when computing an exact duration.When computing an exact duration, the
        // greatest order time components MUST be added first, that is, the number of days MUST be
        // added first, followed by the number of hours, number of minutes, and number of seconds.

        (TimeSpan? nominalPart, TimeSpan? exactPart) dt;
        if (calDateTime.TzId is null)
            dt = (d.ToTimeSpanUnspecified(), null);
        else
            dt = (d.HasDate ? d.DateAsTimeSpan : null, d.HasTime ? d.TimeAsTimeSpan : null);

        var newDateTime = calDateTime;
        if (dt.nominalPart is not null)
            newDateTime = new CalDateTime(newDateTime.Value.Add(dt.nominalPart.Value), calDateTime.TzId, calDateTime.HasTime);

        if (dt.exactPart is not null)
            newDateTime = new CalDateTime(AsUtc(newDateTime).Add(dt.exactPart.Value), CalDateTime.UtcTzId, calDateTime.HasTime);

        if (calDateTime.TzId is not null)
            // Convert to the original timezone even if already set to ensure we're not in a non-existing time.
            newDateTime = ToTimeZone(newDateTime, calDateTime.TzId);

        return newDateTime;
    }

    public static string ToString(this CalDateTime calDateTime, string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        var dateTimeOffset =
            ResolveZonedDateTime(calDateTime)?.ToDateTimeOffset()
            ?? DateUtil.ToZonedDateTimeLeniently(calDateTime.Value, calDateTime.TzId ?? string.Empty).ToDateTimeOffset();

        // Use the .NET format options to format the DateTimeOffset
        var tzIdString = calDateTime.TzId is not null ? $" {calDateTime.TzId}" : string.Empty;

        return calDateTime.HasTime
            ? $"{dateTimeOffset.ToString(format, formatProvider)}{tzIdString}"
            : $"{DateOnly.FromDateTime(dateTimeOffset.Date).ToString(format ?? "d", formatProvider)}{tzIdString}";
    }

    internal static CalDateTime Copy(this CalDateTime calDateTime)
    {
        var copy = new CalDateTime(calDateTime.Date, calDateTime.Time, calDateTime.TzId);

        if (calDateTime.IsFloating)
            return copy;

        AddToCache(copy, AsZonedDateTime(calDateTime));
        return copy;
    }

    /// <summary>
    /// Returns the NodaTime ZonedDateTime for the given CalDateTime.
    /// </summary>
    /// <exception cref="InvalidOperationException">Undefined for floating date/time.</exception>
    internal static ZonedDateTime AsZonedDateTime(this CalDateTime calDateTime) => ResolveZonedDateTime(calDateTime) ?? throw new InvalidOperationException("Timezone not found.");

    /// <summary>Returns a new <see cref="TimeSpan" /> from subtracting the specified <see cref="CalDateTime"/> from to the value of this instance.</summary>
    public static TimeSpan SubtractExact(this CalDateTime dt, CalDateTime other) => dt.AsUtc() - other.AsUtc();

    /// <summary>
    /// Returns a new <see cref="DataTypes.Duration"/> from subtracting the specified <see cref="CalDateTime"/> from to the value of this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static DataTypes.Duration Subtract(this CalDateTime dt, CalDateTime other)
    {
        if (dt.TzId is not null)
            return SubtractExact(dt, other).ToDurationExact();

        if (dt.HasTime != other.HasTime)
            throw new InvalidOperationException($"Trying to calculate the difference between dates of different types. An instance of type DATE cannot be subtracted from a DATE-TIME and vice versa: {dt.ToString()} - {other.ToString()}");

        return (dt.Value - other.Value).ToDuration();
    }

    /// <inheritdoc cref="DateTime.AddYears"/>
    public static CalDateTime AddYears(this CalDateTime dt, int years)
        => new(dt.Date.AddYears(years), dt.Time, dt.TzId);

    /// <inheritdoc cref="DateTime.AddMonths"/>
    public static CalDateTime AddMonths(this CalDateTime dt, int months)
        => new(dt.Date.AddMonths(months), dt.Time, dt.TzId);

    /// <inheritdoc cref="DateTime.AddDays"/>
    public static CalDateTime AddDays(this CalDateTime dt, int days)
        => new(dt.Date.AddDays(days), dt.Time, dt.TzId);

    /// <inheritdoc cref="DateTime.AddHours"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime AddHours(this CalDateTime calDateTime, int hours) => Add(calDateTime, DataTypes.Duration.FromHours(hours));

    /// <inheritdoc cref="DateTime.AddMinutes"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime AddMinutes(this CalDateTime calDateTime, int minutes) => Add(calDateTime, DataTypes.Duration.FromMinutes(minutes));

    /// <inheritdoc cref="DateTime.AddSeconds"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime AddSeconds(this CalDateTime calDateTime, int seconds) => Add(calDateTime,DataTypes.Duration.FromSeconds(seconds));
}
