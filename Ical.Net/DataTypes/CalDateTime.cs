//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using System;
using System.Globalization;
using System.IO;

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
public sealed class CalDateTime : EncodableDataType, IDateTime
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
    public CalDateTime() { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class.
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see cref="IDateTime.HasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see cref="IDateTime.HasTime"/> is <see langword="false"/>.
    /// </summary>
    /// <param name="value"></param>
    public CalDateTime(IDateTime value)
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
    /// has <see cref="DateTimeKind.Utc"/> and <paramref name="hasTime"/> is <see langword="true"/>.
    /// Otherwise <see cref="TzId"/> will be <see langword="null"/>.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value. Its <see cref="DateTimeKind"/> will be ignored.</param>
    /// <param name="hasTime">
    /// The instance will represent an RFC 5545, Section 3.3.5, DATE-TIME value, if <see paramref="hasTime"/> is <see langword="true"/>.
    /// It will represent an RFC 5545, Section 3.3.4, DATE value, if <see paramref="hasTime"/> is <see langword="false"/>.
    /// </param>
    public CalDateTime(DateTime value, bool hasTime = true) : this(value, value.Kind == DateTimeKind.Utc ? UtcTzId : null, hasTime)
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
        CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable
                 ?? throw new InvalidOperationException("Failure deserializing value"));
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

    /// <inheritdoc/>
    public override ICalendarObject? AssociatedObject
    {
        get => base.AssociatedObject;
        set
        {
            if (!Equals(AssociatedObject, value))
            {
                base.AssociatedObject = value;
            }
        }
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        if (obj is not CalDateTime calDt)
            return;

        base.CopyFrom(obj);

        // Maintain the private date/time backing fields
        _dateOnly = calDt._dateOnly;
        _timeOnly = TruncateTimeToSeconds(calDt._timeOnly);
        _tzId = calDt._tzId;

        AssociateWith(calDt);
    }

    public bool Equals(CalDateTime other) => this == other;

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
            hashCode = (hashCode * 397) ^ AsUtc.GetHashCode();
            hashCode = (hashCode * 397) ^ (TzId != null ? TzId.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator <(CalDateTime? left, IDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating) ? left.Value < right.Value : left.AsUtc < right.AsUtc);
    }

    public static bool operator >(CalDateTime? left, IDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating) ? left.Value > right.Value : left.AsUtc > right.AsUtc);
    }

    public static bool operator <=(CalDateTime? left, IDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating) ? left.Value <= right.Value : left.AsUtc <= right.AsUtc);
    }

    public static bool operator >=(CalDateTime? left, IDateTime? right)
    {
        return left != null
               && right != null
               && ((left.IsFloating || right.IsFloating) ? left.Value >= right.Value : left.AsUtc >= right.AsUtc);
    }

    public static bool operator ==(CalDateTime? left, IDateTime? right)
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

    public static bool operator !=(CalDateTime? left, IDateTime? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a new instance of <see cref="CalDateTime"/> with <see langword="true"/> for <see cref="HasTime"/>
    /// </summary>
    public static implicit operator CalDateTime(DateTime left)
    {
        return new CalDateTime(left);
    }

    /// <inheritdoc/>
    public DateTime AsUtc => DateTime.SpecifyKind(ToTimeZone(UtcTzId).Value, DateTimeKind.Utc);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool IsFloating => _tzId is null;

    /// <inheritdoc/>
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

    /// <inheritdoc cref="TzId"/>
    /// <remarks>This is an alias for <see cref="TzId"/></remarks>
    public string? TimeZoneName => TzId;

    /// <inheritdoc cref="DateTime.Year"/>
    public int Year => Value.Year;

    /// <inheritdoc cref="DateTime.Month"/>
    public int Month => Value.Month;

    /// <inheritdoc cref="DateTime.Day"/>
    public int Day => Value.Day;

    /// <inheritdoc cref="DateTime.Hour"/>
    public int Hour => Value.Hour;

    /// <inheritdoc cref="DateTime.Minute"/>
    public int Minute => Value.Minute;

    /// <inheritdoc cref="DateTime.Second"/>
    public int Second => Value.Second;

    /// <inheritdoc cref="DateTime.DayOfWeek"/>
    public DayOfWeek DayOfWeek => Value.DayOfWeek;

    /// <inheritdoc cref="DateTime.DayOfYear"/>
    public int DayOfYear => Value.DayOfYear;

    /// <inheritdoc cref="DateOnly"/>
    public DateOnly Date => _dateOnly;

    /// <inheritdoc cref="TimeOnly"/>
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
    /// Any <see cref="Time"/> values are truncated to seconds, because
    /// RFC 5545, Section 3.3.5 does not allow for fractional seconds.
    /// </summary>
    private static TimeOnly? TruncateTimeToSeconds(DateTime dateTime) => new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second);

    /// <inheritdoc/>
    IDateTime IDateTime.ToTimeZone(string? otherTzId) => ToTimeZone(otherTzId);

    public CalDateTime ToTimeZone(string? otherTzId)
    {
        if (otherTzId is null)
            return new CalDateTime(Value, null, HasTime);

        ZonedDateTime converted;
        if (IsFloating)
        {
            // Make sure, we properly fix the time if it dosen't exist in the target tz.
            converted = DateUtil.ToZonedDateTimeLeniently(Value, otherTzId);
        }
        else
        {
            var zonedOriginal = DateUtil.ToZonedDateTimeLeniently(Value, TzId);
            converted = zonedOriginal.WithZone(DateUtil.GetZone(otherTzId));
        }

        return converted.Zone == DateTimeZone.Utc
            ? new CalDateTime(converted.ToDateTimeUtc(), UtcTzId)
            : new CalDateTime(converted.ToDateTimeUnspecified(), otherTzId);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public IDateTime Add(Duration d)
    {
        if (!HasTime && d.HasTime)
        {
            throw new InvalidOperationException("This instance represents a 'date-only' value. Only multiples of full days can be added to a 'date-only' instance.");
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
        if (TzId is null)
            dt = (d.ToTimeSpan(), null);
        else
            dt = (d.HasDate ? d.DateAsTimeSpan : null, d.HasTime ? d.TimeAsTimeSpan : null);

        var newDateTime = this;
        if (dt.nominalPart is not null)
            newDateTime = new CalDateTime(newDateTime.Value.Add(dt.nominalPart.Value), TzId, HasTime);

        if (dt.exactPart is not null)
            newDateTime = new CalDateTime(newDateTime.AsUtc.Add(dt.exactPart.Value), UtcTzId, HasTime);
        
        if (TzId is not null)
            // Convert to the original timezone even if already set to ensure we're not in a non-existing time.
            newDateTime = newDateTime.ToTimeZone(TzId);

        AssociateWith(this);

        return newDateTime;
    }

    /// <inheritdoc/>
    public TimeSpan SubtractExact(IDateTime dt) => AsUtc - dt.AsUtc;

    /// <inheritdoc/>
    public Duration Subtract(IDateTime dt)
    {
        if (this.TzId is not null)
            return SubtractExact(dt).ToDurationExact();

        if (dt.HasTime != HasTime)
            throw new InvalidOperationException("Trying to calculate the difference between dates of different types. An instance of type DATE cannot be subtracted from a DATE-TIME and vice versa.");

        return (Value - dt.Value).ToDuration();
    }

    /// <inheritdoc cref="DateTime.AddYears"/>
    public IDateTime AddYears(int years)
    {
        var dt = Copy<CalDateTime>();
        dt._dateOnly = dt._dateOnly.AddYears(years);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddMonths"/>
    public IDateTime AddMonths(int months)
    {
        var dt = Copy<CalDateTime>();
        dt._dateOnly = dt._dateOnly.AddMonths(months);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddDays"/>
    public IDateTime AddDays(int days)
    {
        var dt = Copy<CalDateTime>();
        dt._dateOnly = dt._dateOnly.AddDays(days);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddHours"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public IDateTime AddHours(int hours) => Add(Duration.FromHours(hours));

    /// <inheritdoc cref="DateTime.AddMinutes"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public IDateTime AddMinutes(int minutes) => Add(Duration.FromMinutes(minutes));

    /// <inheritdoc cref="DateTime.AddSeconds"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a time span to a date-only instance, 
    /// and the time span is not a multiple of full days.
    /// </exception>
    public IDateTime AddSeconds(int seconds) => Add(Duration.FromSeconds(seconds));

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="IDateTime"/> instance is less than <paramref name="dt"/>.
    /// </summary>
    public bool LessThan(IDateTime dt) => this < dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="IDateTime"/> instance is greater than <paramref name="dt"/>.
    /// </summary>
    public bool GreaterThan(IDateTime dt) => this > dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="IDateTime"/> instance is less than or equal to <paramref name="dt"/>.
    /// </summary>
    public bool LessThanOrEqual(IDateTime dt) => this <= dt;

    /// <summary>
    /// Returns <see langword="true"/> if the current <see cref="IDateTime"/> instance is greater than or equal to <paramref name="dt"/>.
    /// </summary>
    public bool GreaterThanOrEqual(IDateTime dt) => this >= dt;

    /// <summary>
    /// Associates the current instance with the specified <see cref="IDateTime"/> object.
    /// </summary>
    /// <param name="dt">The <see cref="IDateTime"/> object to associate with.</param>
    public void AssociateWith(IDateTime? dt)
    {
        if (AssociatedObject == null && dt?.AssociatedObject != null)
        {
            AssociatedObject = dt.AssociatedObject;
        }
        else if (AssociatedObject != null && dt?.AssociatedObject == null && dt != null)
        {
            dt.AssociatedObject = AssociatedObject;
        }
    }

    /// <summary>
    /// Compares the current instance with another <see cref="IDateTime"/> object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other IDateTime.
    /// </summary>
    /// <param name="dt">The <see cref="IDateTime"/> object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
    /// Less than zero: This instance is less than <paramref name="dt"/>.
    /// Zero: This instance is equal to <paramref name="dt"/>.
    /// Greater than zero: This instance is greater than <paramref name="dt"/>.
    /// </returns>
    public int CompareTo(IDateTime? dt)
    {
        if (Equals(dt))
        {
            return 0;
        }

        if (dt == null)
        {
            return 1;
        }

        if (this < dt)
        {
            return -1;
        }

        // Meaning "this > dt"
        return 1;
    }

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format) => ToString(format, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        var dateTimeOffset = DateUtil.ToZonedDateTimeLeniently(Value, _tzId).ToDateTimeOffset();

        // Use the .NET format options to format the DateTimeOffset
        var tzIdString = _tzId is not null ? $" {_tzId}" : string.Empty;

        if (HasTime)
        {
            return $"{dateTimeOffset.ToString(format, formatProvider)}{tzIdString}";
        }

        // No time part
        return $"{DateOnly.FromDateTime(dateTimeOffset.Date).ToString(format ?? "d", formatProvider)}{tzIdString}";
    }
}
