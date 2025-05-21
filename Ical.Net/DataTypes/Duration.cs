//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an iCalendar DURATION.
/// </summary>
public struct Duration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Duration"/> struct.
    /// </summary>
    /// <remarks>All non-null arguments must have the same sign.</remarks>
    /// <exception cref="ArgumentException">Thrown if not all non-null arguments have the same sign.</exception>
    public Duration(int? weeks = null, int? days = null, int? hours = null, int? minutes = null, int? seconds = null)
    {
        weeks = NullIfZero(weeks);
        days = NullIfZero(days);
        hours = NullIfZero(hours);
        minutes = NullIfZero(minutes);
        seconds = NullIfZero(seconds);

        var sign = GetSign(weeks) ?? GetSign(days) ?? GetSign(hours) ?? GetSign(minutes) ?? GetSign(seconds) ?? 1;
        if (
            ((GetSign(weeks) ?? sign) != sign)
            || ((GetSign(days) ?? sign) != sign)
            || ((GetSign(hours) ?? sign) != sign)
            || ((GetSign(minutes) ?? sign) != sign)
            || ((GetSign(seconds) ?? sign) != sign))
        {
            throw new ArgumentException("All parts of a duration must have the same sign.");
        }

        Weeks = weeks;
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
    }

    /// <summary>
    /// Gets the number of weeks.
    /// </summary>
    public int? Weeks { get; private set; }

    /// <summary>
    /// Gets the number of days.
    /// </summary>
    public int? Days { get; private set; }

    /// <summary>
    /// Gets the number of hours.
    /// </summary>
    public int? Hours { get; private set; }

    /// <summary>
    /// Gets the number of minutes.
    /// </summary>
    public int? Minutes { get; private set; }

    /// <summary>
    /// Gets the number of seconds.
    /// </summary>
    public int? Seconds { get; private set; }

    /// <summary>
    /// Returns +1 if the duration is positive or zero, -1 if the duration is negative.
    /// </summary>
    public int Sign => GetSign(Weeks) ?? GetSign(Days) ?? GetSign(Hours) ?? GetSign(Minutes) ?? GetSign(Seconds) ?? 1;

    /// <summary>
    /// Gets a value indicating whether this instance has a date component.
    /// </summary>
    public bool HasDate => ((Weeks ?? 0) != 0) || ((Days ?? 0) != 0);

    /// <summary>
    /// Gets a value indicating whether this instance has a time component.
    /// </summary>
    public bool HasTime => ((Hours ?? 0) != 0) || ((Minutes ?? 0) != 0) || ((Seconds ?? 0) != 0);

    /// <summary>
    /// Gets an instance representing a duration of zero.
    /// </summary>
    public static Duration Zero { get; } = new Duration();

    /// <summary>
    /// Gets an instance representing a duration of the given number of weeks.
    /// </summary>
    public static Duration FromWeeks(int weeks) =>
        new Duration(weeks: weeks);

    /// <summary>
    /// Gets an instance representing a duration of the given number of days.
    /// </summary>
    public static Duration FromDays(int days) =>
        new Duration(days: days);

    /// <summary>
    /// Gets an instance representing a duration of the given number of hours.
    /// </summary>
    public static Duration FromHours(int hours) =>
        new Duration(hours: hours);

    /// <summary>
    /// Gets an instance representing a duration of the given number of minutes.
    /// </summary>
    public static Duration FromMinutes(int minutes) =>
        new Duration(minutes: minutes);

    /// <summary>
    /// Gets an instance representing a duration of the given number of seconds.
    /// </summary>
    public static Duration FromSeconds(int seconds) =>
        new Duration(seconds: seconds);

    /// <summary>
    /// Parses the specified value according to RFC 5545.
    /// </summary>
    /// <exception cref="System.FormatException">Thrown if the value is not a valid duration.</exception>
    public static Duration? Parse(string value) =>
        (Duration?) new DurationSerializer().Deserialize(new StringReader(value))!;

    /// <summary>
    /// Creates an instance that represents the given time span as exact value, that is, time-only.
    /// </summary>
    /// <remarks>
    /// According to RFC5545 the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    public static Duration FromTimeSpanExact(TimeSpan t)
        // As a TimeSpan always refers to exact time, we specify days as part of the hours field,
        // because time is added as exact values rather than nominal according to RFC 5545.
        => new Duration(hours: NullIfZero(t.Days * 24 + t.Hours), minutes: NullIfZero(t.Minutes), seconds: NullIfZero(t.Seconds));

    /// <summary>
    /// Creates an instance that represents the given time span, treating the days as nominal duration and the time part as exact.
    /// </summary>
    /// <remarks>
    /// According to RFC5545 the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    internal static Duration FromTimeSpan(TimeSpan t)
        => new Duration(days: NullIfZero(t.Days), hours: NullIfZero(t.Hours), minutes: NullIfZero(t.Minutes), seconds: NullIfZero(t.Seconds));

    /// <summary>
    /// Gets a value representing the time parts of the given instance.
    /// </summary>
    internal TimeSpan TimeAsTimeSpan => new(0, Hours ?? 0, Minutes ?? 0, Seconds ?? 0);

    /// <summary>
    /// Gets a value representing the date parts (days and weeks) of the given instance.
    /// </summary>
    internal TimeSpan DateAsTimeSpan => new((Weeks ?? 0) * 7 + (Days ?? 0), 0, 0, 0);

    /// <summary>
    /// Convert the instance to a <see cref="TimeSpan"/>, ignoring potential
    /// DST changes.
    /// </summary>
    /// <remarks>
    /// A duration's days and weeks are considered nominal durations, while the time fields are
    /// considered exact values.
    /// To convert a duration to a <see cref="TimeSpan"/> while considering the days and weeks as
    /// nominal durations, use <see cref="ToTimeSpan"/>.
    /// </remarks>
    public TimeSpan ToTimeSpanUnspecified()
        => new TimeSpan((Weeks ?? 0) * 7 + (Days ?? 0), Hours ?? 0, Minutes ?? 0, Seconds ?? 0);

    /// <summary>
    /// Convert the instance to a <see cref="TimeSpan"/>, treating the days as nominal duration and
    /// the time part as exact.
    /// </summary>
    /// <remarks>
    /// A duration's days and weeks are considered nominal durations, while the time fields are considered exact values.
    /// To convert a duration to a <see cref="TimeSpan"/> while considering the days and weeks as nominal durations,
    /// use <see cref="ToTimeSpan"/>.
    /// </remarks>
    public TimeSpan ToTimeSpan(CalDateTime start)
        => start.Add(this).SubtractExact(start);

    /// <summary>
    /// Gets a value indicating whether the duration is zero, that is, all fields are null or 0.
    /// </summary>
    internal bool IsZero
        => ((Weeks ?? 0) == 0)
            && ((Days ?? 0) == 0)
            && ((Hours ?? 0) == 0)
            && ((Minutes ?? 0) == 0)
            && ((Seconds ?? 0) == 0);

    /// <summary>
    /// Gets a value indicating whether the duration is empty, that is, all fields are null.
    /// </summary>
    internal bool IsEmpty
        => (Weeks == null)
            && (Days == null)
            && (Hours == null)
            && (Minutes == null)
            && (Seconds == null);

    /// <summary>
    /// Returns a negated copy of the given instance.
    /// </summary>
    public static Duration operator -(Duration d) =>
        new Duration(-d.Weeks, -d.Days, -d.Hours, -d.Minutes, -d.Seconds);

    /// <inheritdoc/>
    public override string? ToString()
        => new DurationSerializer().SerializeToString(this);

    private static int? GetSign(int? v) =>
        v switch
        {
            null => null,
            >= 0 => 1,
            < 0 => -1
        };

    private static int? NullIfZero(int? v) => (v == 0) ? null : v;
}
