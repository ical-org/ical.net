//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

/// <summary>
/// This class belongs to the evaluation layer of the library.
/// It provides extension methods for the <see cref="CalDateTime"/> class
/// </summary>
public static class CalDateTimeExtensions
{
    /// <summary>
    /// Converts the <see cref="CalDateTime"/> to the UTC timezone.
    /// If <see cref="CalDateTime.IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="CalDateTime.Value"/> is considered as local time for every timezone:
    /// The returned <see cref="CalDateTime.Value"/> is unchanged, but with <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    public static DateTime AsUtc(this CalDateTime calDateTime)
        => calDateTime.AsZoned().ToTimeZone(CalDateTime.UtcTzId).Utc!.Value;

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
        => calDateTime.AsZoned().ToTimeZone(otherTzId).CalDateTime;

    /// <summary>
    /// Add the specified <see cref="DataTypes.Duration"/> to this instance (timezone-aware)./>.
    /// </summary>
    /// <remarks>
    /// In correspondence to RFC5545, the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime Add(this CalDateTime calDateTime, Duration d)
        => calDateTime.AsZoned().Add(d).CalDateTime;

    /// <summary>
    /// Returns a new <see cref="TimeSpan" /> from subtracting the specified <see cref="CalDateTime"/>
    /// from the value of this instance (timezone-aware).</summary>
    public static TimeSpan SubtractExact(this CalDateTime dt, CalDateTime other)
        => dt.AsZoned().ToTimeZone(CalDateTime.UtcTzId).Utc!.Value - other.AsZoned().ToTimeZone(CalDateTime.UtcTzId).Utc!.Value;

    /// <summary>
    /// Returns a new <see cref="DataTypes.Duration"/> from subtracting the specified <see cref="CalDateTime"/>
    /// from the value of this instance (timezone-aware).
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static Duration Subtract(this CalDateTime dt, CalDateTime other)
        => dt.AsZoned().Subtract(other.AsZoned());

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of years to the value of this instance.
    /// </summary>
    public static CalDateTime AddYears(this CalDateTime dt, int years)
        => dt.AsZoned().AddYears(years).CalDateTime;

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of months to the value of this instance.
    /// </summary>
    public static CalDateTime AddMonths(this CalDateTime dt, int months)
        => dt.AsZoned().AddMonths(months).CalDateTime;

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of days to the value of this instance.
    /// </summary>
    public static CalDateTime AddDays(this CalDateTime dt, int days)
        => dt.AsZoned().AddDays(days).CalDateTime;

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of hours to the value of this instance (timezone-aware).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws when attempting to add a time span to a date-only instance.
    /// </exception>
    public static CalDateTime AddHours(this CalDateTime dt, int hours)
        => dt.AsZoned().AddHours(hours).CalDateTime;

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of minutes to the value of this instance (timezone-aware).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime AddMinutes(this CalDateTime dt, int minutes)
        => dt.AsZoned().AddMinutes(minutes).CalDateTime;

    /// <summary>
    /// Returns a new instance that adds the specified number
    /// of seconds to the value of this instance (timezone-aware).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public static CalDateTime AddSeconds(this CalDateTime dt, int seconds)
        => dt.AsZoned().AddSeconds(seconds).CalDateTime;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/>
    /// instance is less than <paramref name="dt"/> (timezone-aware).
    /// </summary>
    public static bool LessThan(this CalDateTime dt, CalDateTime? other) => dt.AsZoned().LessThan(other?.AsZoned());

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/>
    /// instance is greater than <paramref name="dt"/> (timezone-aware).
    /// </summary>
    public static bool GreaterThan(this CalDateTime dt, CalDateTime? other) => dt.AsZoned().GreaterThan(other?.AsZoned());

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/>
    /// instance is less than or equal to <paramref name="dt"/> (timezone-aware).
    /// </summary>
    public static bool LessThanOrEqual(this CalDateTime dt, CalDateTime? other) => dt.AsZoned().LessThanOrEqual(other?.AsZoned());

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/>
    /// instance is greater than or equal to <paramref name="dt"/> (timezone-aware).
    /// </summary>
    public static bool GreaterThanOrEqual(this CalDateTime dt, CalDateTime? other) => dt.AsZoned().GreaterThanOrEqual(other?.AsZoned());
}
