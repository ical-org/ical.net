//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.Text.RegularExpressions;
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

    public static Duration FromPeriod(NodaTime.Period p)
        => new(p.Weeks, p.Days, (int) p.Hours, (int) p.Minutes, (int) p.Seconds);

    public readonly NodaTime.Period ToPeriod()
    {
        var b = new NodaTime.PeriodBuilder
        {
            Weeks = Weeks ?? 0,
            Days = Days ?? 0,
            Hours = Hours ?? 0,
            Minutes = Minutes ?? 0,
            Seconds = Seconds ?? 0
        };

        return b.Build();
    }

    public readonly NodaTime.Period GetNominalPart()
    {
        var b = new NodaTime.PeriodBuilder
        {
            Weeks = Weeks ?? 0,
            Days = Days ?? 0,
        };

        return b.Build();
    }

    public readonly NodaTime.Duration GetTimePart()
    {
        var b = new NodaTime.PeriodBuilder
        {
            Hours = Hours ?? 0,
            Minutes = Minutes ?? 0,
            Seconds = Seconds ?? 0,
        };

        return b.Build().ToDuration();
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
    public static Duration? Parse(string value)
    {
        return TryParseInternal(value, out var duration) switch
        {
            DurationParseResult.Success => duration,
            DurationParseResult.NoMatch => null,
            DurationParseResult.Invalid or _ =>
                throw new FormatException("String value is not in the ISO 8601 basic format for DURATION")
        };
    }

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
    public override string? ToString() => ToBasicIso();

    private static int? GetSign(int? v) =>
        v switch
        {
            null => null,
            >= 0 => 1,
            < 0 => -1
        };

    private static int? NullIfZero(int? v) => (v == 0) ? null : v;


    internal static readonly Regex DurationMatch =
        new Regex(@"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexDefaults.Timeout);

    /// <summary>
    /// Parse result for backwards compatibility with <see cref="Parse(string)"/>
    /// </summary>
    internal enum DurationParseResult
    {
        Success,
        NoMatch,
        Invalid,
    }

    public static bool TryParse(string? value, out Duration duration)
        => DurationParseResult.Success == TryParseInternal(value, out duration);

    internal static DurationParseResult TryParseInternal(string? value, out Duration duration)
    {
        if (string.IsNullOrEmpty(value))
        {
            duration = default;
            return DurationParseResult.NoMatch;
        }

        try
        {
            var match = DurationMatch.Match(value);

            if (!match.Success)
            {
                // This happens for
                // * EXDATE values, which never have a duration,
                // * RDATE values, where the optional duration is missing
                // * TRIGGER values that are invalid
                //return null;

                duration = default;
                return DurationParseResult.NoMatch;
            }

            var sign = 1;
            int? weeks = null;
            int? days = null;
            int? hours = null;
            int? minutes = null;
            int? seconds = null;

            if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                sign = -1;

            int? GetGroupInt(string key)
                => match.Groups[key].Success ? Convert.ToInt32(match.Groups[key].Value, CultureInfo.InvariantCulture) : null;

            weeks = GetGroupInt("week");
            if (match.Groups["main"].Success)
            {
                days = GetGroupInt("day");
                if (match.Groups["time"].Success)
                {
                    hours = GetGroupInt("hour");
                    minutes = GetGroupInt("minute");
                    seconds = GetGroupInt("second");
                }
            }

            duration = new Duration(sign * weeks, sign * days, sign * hours, sign * minutes, sign * seconds);
            return DurationParseResult.Success;
        }
        catch (Exception)
        {
            duration = default;
            return DurationParseResult.Invalid;
        }
    }

    internal string ToBasicIso()
    {
        if (IsEmpty)
        {
            return "P0D";
        }

        // Max string length is:
        // sign: 1 ch
        // 'P': 1 ch
        // 'T': 1 ch
        // values: 10 * 5 ch (int.MaxValue.ToString.Length())
        // units: 1 * 5 ch
        const int MaxDurationStringLength = 58;

        var written = 0;
        Span<char> result = stackalloc char[MaxDurationStringLength];

        var sign = Sign;
        if (sign < 0)
            result[written++] = '-';

        result[written++] = 'P';

        if (Weeks is { } weeks)
            written += WriteInt(sign * weeks, 'W', result.Slice(written));

        if (Days is { } days)
            written += WriteInt(sign * days, 'D', result.Slice(written));

        if (Hours != null || Minutes != null || Seconds != null)
        {
            result[written++] = 'T';

            if (Hours is { } hours)
                written += WriteInt(sign * hours, 'H', result.Slice(written));

            if (Minutes is { } minutes)
                written += WriteInt(sign * minutes, 'M', result.Slice(written));

            if (Seconds is { } seconds)
                written += WriteInt(sign * seconds, 'S', result.Slice(written));
        }

        return result.Slice(0, written).ToString();

        static int WriteInt(int value, char unit, Span<char> buffer)
        {
#if NET
            if (!value.TryFormat(buffer, out var written, null, CultureInfo.InvariantCulture))
            {
                throw new InvalidOperationException("Integer value could not be formatted to string");
            }
#else
            var strValue = value.ToString(CultureInfo.InvariantCulture);
            strValue.AsSpan().CopyTo(buffer);
            var written = strValue.Length;
#endif
            buffer[written++] = unit;
            return written;
        }
    }

}
