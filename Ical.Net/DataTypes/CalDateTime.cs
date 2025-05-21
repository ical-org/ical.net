//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.Serialization.DataTypes;
using System;
using System.IO;
using Ical.Net.Evaluation;

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
public sealed class CalDateTime : IComparable<CalDateTime>, IFormattable
{
    // The date part that is used to return the Value property.
    private DateOnly _dateOnly;
    // The time part that is used to return the Value property.
    private TimeOnly? _timeOnly;

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
    private CalDateTime()
    {
        // required for the SerializerFactory to work
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see cref="CalDateTime.HasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see cref="CalDateTime.HasTime"/> is <see langword="false"/>.
    /// </summary>
    /// <param name="value"></param>
    public CalDateTime(CalDateTime value)
    {
        if (value.HasTime)
            Initialize(DateOnly.FromDateTime(value.Value), TimeOnly.FromDateTime(value.Value), value.TzId);
        else
            Initialize(DateOnly.FromDateTime(value.Value), null, value.TzId);
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
    public CalDateTime(DateTime value, string? tzId, bool hasTime = true)
    {
        if (hasTime)
            Initialize(DateOnly.FromDateTime(value), TimeOnly.FromDateTime(value), tzId);
        else
            Initialize(DateOnly.FromDateTime(value), null, tzId);
    }

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
    {
        Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), tzId);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class with <see cref="TzId"/> set to <see langword="null"/>.
    /// The instance will represent an RFC 5545, Section 3.3.4, DATE value,
    /// and thus it cannot have a timezone.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    public CalDateTime(int year, int month, int day)
    {
        Initialize(new DateOnly(year, month, day), null, null);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class with <see cref="TzId"/> set to <see langword="null"/>.
    /// The instance will represent an RFC 5545, Section 3.3.4, DATE value,
    /// and thus it cannot have a timezone.
    /// </summary>
    /// <param name="date"></param>
    public CalDateTime(DateOnly date)
    {
        Initialize(date, null, null);
    }

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
    public CalDateTime(DateOnly date, TimeOnly? time, string? tzId = null)
    {
        Initialize(date, time, tzId);
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
        CopyFrom(serializer.Deserialize(new StringReader(value)) as CalDateTime
                 ?? throw new InvalidOperationException($"$Failure for deserializing value '{value}'"));
        // The string may contain a date only, meaning that the tzId should be ignored.
        _tzId = HasTime ? tzId : null;
    }

    private void Initialize(DateOnly dateOnly, TimeOnly? timeOnly, string? tzId)
    {
        _dateOnly = dateOnly;
        _timeOnly = TruncateTimeToSeconds(timeOnly);

        _tzId = tzId switch
        {
            _ when !timeOnly.HasValue => null,
            _ => tzId // can also be UtcTzId
        };
    }

    private void CopyFrom(CalDateTime calDt)
    {
        // Maintain the private date/time backing fields
        _dateOnly = calDt._dateOnly;
        _timeOnly = calDt._timeOnly;
        _tzId = calDt._tzId;
    }

    public bool Equals(CalDateTime? other) => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is CalDateTime other && this == other;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Value.GetHashCode();
            hashCode = (hashCode * 397) ^ HasTime.GetHashCode();
            hashCode = (hashCode * 397) ^ (TzId != null ? TzId.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator <(CalDateTime? left, CalDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating || left.TzId == right.TzId) ? left.Value < right.Value : left.AsUtc() < right.AsUtc());
    }

    public static bool operator >(CalDateTime? left, CalDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating || left.TzId == right.TzId) ? left.Value > right.Value : left.AsUtc() > right.AsUtc());
    }

    public static bool operator <=(CalDateTime? left, CalDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating || left.TzId == right.TzId) ? left.Value <= right.Value : left.AsUtc() <= right.AsUtc());
    }

    public static bool operator >=(CalDateTime? left, CalDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating || left.TzId == right.TzId) ? left.Value >= right.Value : left.AsUtc() >= right.AsUtc());
    }

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

        if (left.IsFloating != right.IsFloating)
        {
            return false;
        }

        return left.Value.Equals(right.Value)
               && left.HasTime == right.HasTime
               && string.Equals(left.TzId, right.TzId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(CalDateTime? left, CalDateTime? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Gets the date and time value in the ISO calendar as a <see cref="DateTime"/> type with <see cref="DateTimeKind.Unspecified"/>.
    /// The value has no associated timezone.<br/>
    /// The precision of the time part is up to seconds.
    /// <para/>
    /// Use <see cref="IsUtc"/> along with <see cref="TzId"/> and <see cref="IsFloating"/>
    /// to control how this date/time is handled.
    /// </summary>
    public DateTime Value
    {
        get
        {
            if (_timeOnly.HasValue)
            {
                return new DateTime(_dateOnly.Year, _dateOnly.Month,
                    _dateOnly.Day, _timeOnly.Value.Hour, _timeOnly.Value.Minute, _timeOnly.Value.Second,
                    DateTimeKind.Unspecified);
            }

            // No time part
            return new DateTime(_dateOnly.Year, _dateOnly.Month, _dateOnly.Day,
                0, 0, 0,
                DateTimeKind.Unspecified);
        }
    }

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
    public bool HasTime => _timeOnly.HasValue;

    private string? _tzId;

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
    /// Gets the year that applies to the <see cref="Value"/>.
    /// </summary>
    public int Year => Value.Year;

    /// <summary>
    /// Gets the month that applies to the <see cref="Value"/>.
    /// </summary>
    public int Month => Value.Month;

    /// <summary>
    /// Gets the day that applies to the <see cref="Value"/>.
    /// </summary>
    public int Day => Value.Day;

    /// <summary>
    /// Gets the hour that applies to the <see cref="Value"/>.
    /// </summary>
    public int Hour => Value.Hour;

    /// <summary>
    /// Gets the minute that applies to the <see cref="Value"/>.
    /// </summary>
    public int Minute => Value.Minute;

    /// <summary>
    /// Gets the second that applies to the <see cref="Value"/>.
    /// </summary>
    public int Second => Value.Second;

    /// <summary>
    /// Gets the DayOfWeek that applies to the <see cref="Value"/>.
    /// </summary>
    public DayOfWeek DayOfWeek => Value.DayOfWeek;

    /// <summary>
    /// Gets the DayOfYear that applies to the <see cref="Value"/>.
    /// </summary>
    public int DayOfYear => Value.DayOfYear;

    /// <summary>
    /// Gets the date portion of the <see cref="Value"/>.
    /// </summary>
    public DateOnly Date => _dateOnly;

    /// <summary>
    /// Gets the time portion of the <see cref="Value"/>, or <see langword="null"/> if the <see cref="Value"/> is a pure date.
    /// </summary>
    public TimeOnly? Time => _timeOnly;

    /// <summary>
    /// Any <see cref="Time"/> values are truncated to seconds, because
    /// RFC 5545, Section 3.3.5 does not allow for fractional seconds.
    /// </summary>
    private static TimeOnly? TruncateTimeToSeconds(TimeOnly? time)
    {
        if (time is null)
        {
            return null;
        }

        return new TimeOnly(time.Value.Hour, time.Value.Minute, time.Value.Second);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/> instance is less than <paramref name="dt"/>.
    /// </summary>
    public bool LessThan(CalDateTime? dt) => this < dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/> instance is greater than <paramref name="dt"/>.
    /// </summary>
    public bool GreaterThan(CalDateTime? dt) => this > dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/> instance is less than or equal to <paramref name="dt"/>.
    /// </summary>
    public bool LessThanOrEqual(CalDateTime? dt) => this <= dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="CalDateTime"/> instance is greater than or equal to <paramref name="dt"/>.
    /// </summary>
    public bool GreaterThanOrEqual(CalDateTime? dt) => this >= dt;

    /// <summary>
    /// Compares the current instance with another <see cref="CalDateTime"/> object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other CalDateTime.
    /// </summary>
    /// <param name="dt">The <see cref="CalDateTime"/> object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
    /// Less than zero: This instance is less than <paramref name="dt"/>.
    /// Zero: This instance is equal to <paramref name="dt"/>.
    /// Greater than zero: This instance is greater than <paramref name="dt"/>.
    /// </returns>
    public int CompareTo(CalDateTime? dt)
    {
        if (dt == null)
        {
            return 1;
        }

        if (Equals(dt))
        {
            return 0;
        }

        if (this < dt)
        {
            return -1;
        }

        // Meaning "this > dt"
        return 1;
    }

    /// <inheritdoc />
    public override string ToString() => this.ToString(null, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format) => this.ToString(format, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format, IFormatProvider? formatProvider) // part of IFormattable
        => CalDateTimeExtensions.ToString(this, format, formatProvider);
}
