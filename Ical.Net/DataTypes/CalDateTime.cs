//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace Ical.Net.DataTypes;

/// <summary>
/// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
/// <remarks>
/// In addition to the features of the <see cref="DateTime"/> class, the <see cref="CalDateTime"/>
/// class handles timezones, floating date/times and integrates seamlessly into the iCalendar framework.
/// <para/>
/// Any <see cref="Time"/> values are always rounded to the nearest second.
/// This is because RFC 5545, Section 3.3.5, does not allow for fractional seconds.
/// </remarks>
/// </summary>
public sealed class CalDateTime : IFormattable
{
    // The date part that is used to return the Value property.
    private readonly LocalDate _localDate;
    // The time part that is used to return the Value property.
    private readonly LocalTime? _localTime;

    private readonly string? _tzId;


    /// <summary>
    /// The timezone ID for Universal Coordinated Time (UTC).
    /// </summary>
    public const string UtcTzId = "UTC";

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class
    /// with the current date/time and sets the <see cref="TzId"/> to <see langword="null"/>.
    /// </summary>
    public static CalDateTime Now => new CalDateTime(DateTime.Now, null, true);

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class
    /// with the current date and sets the <see cref="TzId"/> to <see langword="null"/>.
    /// </summary>
    public static CalDateTime Today => new CalDateTime(DateTime.Today, null, false);

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class
    /// with the current date/time in the Coordinated Universal Time (UTC) timezone.
    /// </summary>
    public static CalDateTime UtcNow => new CalDateTime(DateTime.UtcNow, UtcTzId, true);

    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private CalDateTime()
    {
        // required for the SerializerFactory to work
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see paramref="hasTime"/> is <see langword="false"/>.
    /// <para/>
    /// The <see cref="TzId"/> will be set to "UTC" if the <paramref name="value"/>
    /// has kind <see cref="DateTimeKind.Utc"/> and <paramref name="hasTime"/> is <see langword="true"/>.
    /// It will be set to <see langword="null"/> if the kind is kind <see cref="DateTimeKind.Unspecified"/>
    /// and will throw otherwise.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value. Its <see cref="DateTimeKind"/> will be ignored.</param>
    /// <param name="hasTime">
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see paramref="hasTime"/> is <see langword="false"/>.
    /// </param>
    /// <exception cref="System.ArgumentException">If the specified value's kind is <see cref="DateTimeKind.Local"/></exception>
    public CalDateTime(DateTime value, bool hasTime = true) : this(
        value,
        value.Kind switch
        {
            DateTimeKind.Utc => UtcTzId,
            DateTimeKind.Unspecified => null,
            _ => throw new ArgumentException($"An instance of {nameof(CalDateTime)} can only be initialized from a {nameof(DateTime)} of kind {nameof(DateTimeKind.Utc)} or {nameof(DateTimeKind.Unspecified)}.")
        },
        hasTime)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see paramref="hasTime"/> is <see langword="false"/>.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value. Its <see cref="DateTimeKind"/> will be ignored.</param>
    /// <param name="tzId">A timezone of <see langword="null"/> represents
    /// a floating date/time, which is the same in all timezones. (<seealso cref="UtcTzId"/>) represents the Coordinated Universal Time.
    /// Other values determine the timezone of the date/time.
    /// </param>
    /// <param name="hasTime">
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see paramref="hasTime"/> is <see langword="false"/>.
    /// </param>
    public CalDateTime(DateTime value, string? tzId, bool hasTime = true) : this(
        LocalDate.FromDateTime(value),
        hasTime ? LocalDateTime.FromDateTime(value).TimeOfDay : null,
        tzId)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified timezone.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value.
    /// </summary>
    /// <param name="tzId">A timezone of <see langword="null"/> represents
    /// a floating date/time, which is the same in all timezones. (<seealso cref="UtcTzId"/>) represents the Coordinated Universal Time.
    /// Other values determine the timezone of the date/time.
    /// </param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    public CalDateTime(int year, int month, int day, int hour, int minute, int second, string? tzId = null) //NOSONAR - must keep this signature
        : this(new LocalDate(year, month, day), new LocalTime(hour, minute, second), tzId)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class with <see cref="TzId"/> set to <see langword="null"/>.
    /// The instance will represent an RFC 5545, Section 3.3.4, DATE value,
    /// and thus it cannot have a timezone.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    public CalDateTime(int year, int month, int day)
        : this(new LocalDate(year, month, day), null, null)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class with <see cref="TzId"/> set to <see langword="null"/>.
    /// The instance will represent an RFC 5545, Section 3.3.4, DATE value,
    /// and thus it cannot have a timezone.
    /// </summary>
    /// <param name="date"></param>
    public CalDateTime(LocalDate date) : this(date, null, null)
    { }

    public CalDateTime(LocalDate value, string? tzId = null)
        : this(value, null, tzId)
    { }

    public CalDateTime(LocalDateTime value, string? tzId = null)
        : this(value.Date, value.TimeOfDay, tzId)
    { }

    /// <summary>
    /// Note that this drops the time zone offset,
    /// so the value is not always exactly the same
    /// when converting back to ZonedDateTime.
    /// </summary>
    /// <param name="value"></param>
    internal CalDateTime(ZonedDateTime value)
        : this(value.LocalDateTime, value.Zone.Id)
    { }

    public CalDateTime(Instant instant) : this(instant.InUtc())
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified timezone.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value.
    /// </summary>
    /// <param name="tzId">A timezone of <see langword="null"/> represents
    /// a floating date/time, which is the same in all timezones. (<seealso cref="UtcTzId"/>) represents the Coordinated Universal Time.
    /// Other values determine the timezone of the date/time.
    /// </param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    public CalDateTime(LocalDate date, LocalTime? time, string? tzId = null)
    {
        // NodaTime supports year values <1 (BCE). Make sure these
        // years are considered invalid.
        if (date.Year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Year must be a positive value");
        }

        _localDate = date;
        _localTime = TruncateTimeToSeconds(time);

        _tzId = tzId switch
        {
            _ when !time.HasValue => null,
            _ => tzId // can also be UtcTzId
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class by parsing <paramref name="value"/>
    /// using the <see cref="DateTimeSerializer"/>.
    /// </summary>
    /// <param name="value">An iCalendar-compatible date or date-time string.
    /// <para/>
    /// If the parsed string represents an RFC 5545, Section 3.3.4, DATE value,
    /// it cannot have a timezone, and the <see paramref="tzId"/> will be ignored.
    /// <para/>
    /// If the parsed string represents an RFC 5545, DATE-TIME value, the <see paramref="tzId"/> will be used.
    /// </param>
    /// <param name="tzId">A timezone of <see langword="null"/> represents
    /// a floating date/time, which is the same in all timezones. (<seealso cref="UtcTzId"/>) represents the Coordinated Universal Time.
    /// Other values determine the timezone of the date/time.
    /// </param>
    public CalDateTime(string value, string? tzId = null)
    {
        var serializer = new DateTimeSerializer();
        var dt = serializer.Deserialize(new StringReader(value)) as CalDateTime
                 ?? throw new InvalidOperationException($"Failure when deserializing value '{value}'");

        _localDate = dt._localDate;
        _localTime = dt._localTime;
        _tzId = dt.IsUtc ? UtcTzId : tzId;

        if (dt.IsUtc && tzId != null && !string.Equals(tzId, UtcTzId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The value '{value}' represents UTC date/time, but the specified timezone '{tzId}' is not '{UtcTzId}'.",
                nameof(tzId));
        }
    }

    public bool Equals(CalDateTime? other) => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is CalDateTime other && this == other;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_localDate, _localTime, _tzId);

    public static bool operator ==(CalDateTime? left, CalDateTime? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left._localDate == right._localDate
            && left._localTime == right._localTime
            && string.Equals(left.TzId, right.TzId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(CalDateTime? left, CalDateTime? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Converts the date/time to UTC (Coordinated Universal Time)
    /// If <see cref="IsFloating"/>==<see langword="true"/>
    /// it means that the value is considered as local time for every timezone:
    /// The returned value is unchanged, but with <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    ///
    public DateTime AsUtc => ToInstant().ToDateTimeUtc();

    /// <summary>
    /// Gets the date and time value in the ISO calendar as a <see cref="DateTime"/> type with <see cref="DateTimeKind.Unspecified"/>.
    /// The value has no associated timezone.<br/>
    /// The precision of the time part is up to seconds.
    /// <para/>
    /// Use <see cref="IsUtc"/> along with <see cref="TzId"/> and <see cref="IsFloating"/>
    /// to control how this date/time is handled.
    /// </summary>
    public DateTime Value => ToLocalDateTime().ToDateTimeUnspecified();

    /// <summary>
    /// Returns <see langword="true"/>, if the date/time value is floating.
    /// <para/>
    /// A floating date/time value does not include a timezone identifier or UTC offset,
    /// so it is interpreted as local time in the context where it is used.
    /// <para/>
    /// A floating date/time value is useful when the exact timezone is not
    /// known or when the event should be interpreted in the local timezone of
    /// the user or system processing the calendar data.
    /// </summary>
    public bool IsFloating => _tzId is null;

    /// <summary>
    /// Gets/sets whether the Value of this date/time represents
    /// a universal time.
    /// </summary>
    public bool IsUtc => string.Equals(_tzId, UtcTzId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <see langword="true"/> if the underlying <see cref="DateTime"/> <see cref="Value"/> has a 'time' part (hour, minute, second).
    /// </summary>
    public bool HasTime => _localTime.HasValue;

    /// <summary>
    /// Gets the timezone ID of this <see cref="CalDateTime"/> instance.
    /// It can be <see cref="UtcTzId"/> for Coordinated Universal Time,
    /// or <see langword="null"/> for a floating date/time, or a value for a specific timezone.
    /// </summary>
    public string? TzId => _tzId;

    /// <summary>
    /// Gets the timezone name this time is in, if it references a timezone.
    /// </summary>
    /// <remarks>This is an alias for <see cref="TzId"/></remarks>
    public string? TimeZoneName => TzId;

    /// <summary>
    /// Gets the year.
    /// </summary>
    public int Year => _localDate.Year;

    /// <summary>
    /// Gets the month.
    /// </summary>
    public int Month => _localDate.Month;

    /// <summary>
    /// Gets the day.
    /// </summary>
    public int Day => _localDate.Day;

    /// <summary>
    /// Gets the hour.
    /// </summary>
    public int Hour => _localTime?.Hour ?? 0;

    /// <summary>
    /// Gets the minute.
    /// </summary>
    public int Minute => _localTime?.Minute ?? 0;

    /// <summary>
    /// Gets the second.
    /// </summary>
    public int Second => _localTime?.Second ?? 0;

    /// <summary>
    /// Gets the DayOfWeek.
    /// </summary>
    public DayOfWeek DayOfWeek => _localDate.DayOfWeek.ToDayOfWeek();

    /// <summary>
    /// Gets the DayOfYear.
    /// </summary>
    public int DayOfYear => _localDate.DayOfYear;

    /// <summary>
    /// Gets the date.
    /// </summary>
    public LocalDate Date => _localDate;

    /// <summary>
    /// Gets the time, or <see langword="null"/> if there is no time.
    /// </summary>
    public LocalTime? Time => _localTime;

#if NET6_0_OR_GREATER
    public CalDateTime(DateOnly date) : this(date, null, null) { }

    public CalDateTime(DateOnly date, TimeOnly? time, string? tzId = null)
        : this(date.ToLocalDate(), time?.ToLocalTime(), tzId) { }

    /// <summary>
    /// Gets the date..
    /// </summary>
    public DateOnly DateOnly => _localDate.ToDateOnly();

    /// <summary>
    /// Gets the time, or <see langword="null"/> if there is no time.
    /// </summary>
    public TimeOnly? TimeOnly => _localTime?.ToTimeOnly();
#endif

    /// <summary>
    /// Any <see cref="Time"/> values are truncated to seconds, because
    /// RFC 5545, Section 3.3.5 does not allow for fractional seconds.
    /// </summary>
    private static LocalTime? TruncateTimeToSeconds(LocalTime? time)
    {
        if (time is null)
        {
            return null;
        }

        return TimeAdjusters.TruncateToSecond(time.Value);
    }

    public LocalDateTime ToLocalDateTime()
    {
        var localDate = new LocalDate(_localDate.Year, _localDate.Month, _localDate.Day);

        if (_localTime is null)
        {
            return localDate.AtMidnight();
        }
        else
        {
            var time = _localTime.Value;
            return localDate.At(new LocalTime(time.Hour, time.Minute, time.Second));
        }
    }

    public Instant ToInstant() => ToZonedDateTime().ToInstant();

    public ZonedDateTime ToZonedDateTime()
    {
        if (_tzId is null)
        {
            return ToLocalDateTime().InUtc();
        }
        else
        {
            return DateUtil.GetZone(_tzId).AtLeniently(ToLocalDateTime());
        }
    }

    public ZonedDateTime ToZonedDateTime(DateTimeZone timeZone)
    {
        if (_tzId is null)
        {
            return ToLocalDateTime().InZoneLeniently(timeZone);
        }
        else
        {
            return DateUtil.GetZone(_tzId)
                .AtLeniently(ToLocalDateTime())
                .WithZone(timeZone);
        }
    }

    public ZonedDateTime ToZonedDateTime(string zoneId)
    {
        return ToZonedDateTime(DateUtil.GetZone(zoneId));
    }

    public ZonedDateTime AsZonedOrDefault(DateTimeZone timeZone)
    {
        if (_tzId is null)
        {
            return ToLocalDateTime().InZoneLeniently(timeZone);
        }
        else
        {
            return DateUtil.GetZone(_tzId).AtLeniently(ToLocalDateTime());
        }
    }

    /// <summary>
    /// Converts the <see cref="Value"/> to a date/time
    /// within the specified <see paramref="otherTzId"/> timezone.
    /// <para/>
    /// If <see cref="IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="Value"/> is considered as local time for every timezone:
    /// The returned <see cref="Value"/> is unchanged and the <see paramref="otherTzId"/> is set as <see cref="TzId"/>.
    /// </summary>
    public CalDateTime ToTimeZone(string? otherTzId)
    {
        if (otherTzId is null)
        {
            return new(_localDate, _localTime);
        }

        return new(ToZonedDateTime(otherTzId));
    }

    /// <summary>
    /// Add the specified <see cref="Duration"/> to this instance/>.
    /// </summary>
    /// <remarks>
    /// In correspondence to RFC5545, the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public CalDateTime Add(Duration d)
    {
        // If NOOP
        if (d.IsEmpty) return this;

        if (!HasTime && d.HasTime)
        {
            throw new InvalidOperationException($"This instance represents a 'date-only' value '{ToString()}'. Only multiples of full days can be added to a 'date-only' instance, not '{d}'");
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

        if (_localTime is null)
        {
            // The value is date only and the duration has no time,
            // so just add to the date.
            var date = ToLocalDateTime().Date.Plus(d.GetNominalPart());
            return new(date, _tzId);
        }
        else
        {
            // Treat floating values as UTC, and add date and time
            var zoned = ToZonedDateTime();
            var result = zoned
                .LocalDateTime
                .Plus(d.GetNominalPart())
                .InZoneLeniently(zoned.Zone)
                .Plus(d.GetTimePart());

            // Use the original time zone (which may be null)
            return new(result.LocalDateTime, _tzId);
        }
    }

    /// <inheritdoc cref="DateTime.AddYears"/>
    public CalDateTime AddYears(int years)
    {
        return new(_localDate.PlusYears(years), _localTime, _tzId);
    }

    /// <inheritdoc cref="DateTime.AddMonths"/>
    public CalDateTime AddMonths(int months)
    {
        return new(_localDate.PlusMonths(months), _localTime, _tzId);
    }

    /// <inheritdoc cref="DateTime.AddDays"/>
    public CalDateTime AddDays(int days)
    {
        return new(_localDate.PlusDays(days), _localTime, _tzId);
    }

    /// <inheritdoc cref="DateTime.AddHours"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public CalDateTime AddHours(int hours) => Add(Duration.FromHours(hours));

    /// <inheritdoc cref="DateTime.AddMinutes"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public CalDateTime AddMinutes(int minutes) => Add(Duration.FromMinutes(minutes));

    /// <inheritdoc cref="DateTime.AddSeconds"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public CalDateTime AddSeconds(int seconds) => Add(Duration.FromSeconds(seconds));

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format) => ToString(format, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        // Use the .NET format options to format
        formatProvider ??= CultureInfo.InvariantCulture;

        // Print only the time zone ID, not the time zone offset.
        // The spec does not allow including a time zone offset to
        // specify a DATE-TIME value, so this should not include it.
        var tzIdString = _tzId is not null ? $" {_tzId}" : string.Empty;

        if (HasTime)
        {
            return ToLocalDateTime().ToString(format, formatProvider) + tzIdString;
        }

        // Handle special case for "O" format that NodaTime.LocalDate
        // does not support. This is the ISO 8601 date format, so it
        // always uses the invariant culture.
        if (format == "O")
        {
            return LocalDatePattern.Iso.Format(_localDate) + tzIdString;
        }

        return _localDate.ToString(format ?? "d", formatProvider) + tzIdString;
    }
}
