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
/// class handles time zones, and integrates seamlessly into the iCalendar framework.
/// </remarks>
/// </summary>
public sealed class CalDateTime : EncodableDataType, IDateTime
{
    // The date and time parts that were used to initialize the instance
    // or by the Value setter.
    private DateTime _value;
    // The date part that is used to return the Value property.
    private DateOnly? _dateOnly;
    // The time part that is used to return the Value property.
    private TimeOnly? _timeOnly;

    const double AlmostZeroEpsilon = 1e-10;

    public static CalDateTime Now => new CalDateTime(DateTime.Now);

    public static CalDateTime Today => new CalDateTime(DateTime.Today);

    public static CalDateTime UtcNow => new CalDateTime(DateTime.UtcNow);

    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    public CalDateTime() { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class
    /// respecting the <see cref="IDateTime.HasTime"/> setting.
    /// </summary>
    /// <param name="value"></param>
    public CalDateTime(IDateTime value)
    {
        Initialize(value.Value, value.HasTime, value.TzId, value.Calendar);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class
    /// and sets the <see cref="TzId"/> to "UTC" if the <paramref name="value"/>
    /// has a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="hasTime">Set to <see langword="true"/> (default), if the <see cref="DateTime.TimeOfDay"/> must be included.</param>
    public CalDateTime(DateTime value, bool hasTime = true) : this(value, value.Kind == DateTimeKind.Utc ? "UTC" : null, hasTime)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
    /// If the time zone specified is UTC, the underlying <see cref="DateTime.Kind"/> will be
    /// <see cref="DateTimeKind.Utc"/>. If a non-UTC time zone is specified, the underlying
    /// <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
    /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be left untouched.</param>
    /// <param name="hasTime">Set to <see langword="true"/> (default), if the <see cref="DateTime.TimeOfDay"/> must be included.</param>
    public CalDateTime(DateTime value, string? tzId, bool hasTime = true)
    {
        Initialize(value, hasTime, tzId, null);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
    /// Sets <see cref="DateTimeKind.Unspecified"/> for the <see cref="Value"/> property.
    /// </summary>
    /// <param name="second"></param>
    /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
    /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
    /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be left untouched.
    /// </param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="cal"></param>
    public CalDateTime(int year, int month, int day, int hour, int minute, int second, string? tzId = null, Calendar? cal = null) //NOSONAR - must keep this signature
    {
        Initialize(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified), true, tzId, cal);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
    /// Sets <see cref="DateTimeKind.Unspecified"/> for the <see cref="Value"/> property.
    /// </summary>
    /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
    /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
    /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be left untouched.
    /// </param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    public CalDateTime(int year, int month, int day, string? tzId = null)
    {
        Initialize(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified), false, tzId, null);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
    /// </summary>
    /// <param name="kind">If <see langword="null"/>, <see cref="DateTimeKind.Unspecified"/> is used.</param>
    /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
    /// If a non-UTC time zone is specified, the underlying <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
    /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be left untouched.
    /// </param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <param name="cal"></param>
    public CalDateTime(DateOnly date, TimeOnly? time, DateTimeKind kind, string? tzId = null, Calendar? cal = null)
    {
        if (time.HasValue)
            Initialize(new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, kind), true, tzId, cal);
        else
            Initialize(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, kind), false, tzId, cal);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CalDateTime"/> class by parsing <paramref name="value"/>
    /// using the <see cref="DateTimeSerializer"/>.
    /// </summary>
    /// <param name="value">An iCalendar-compatible date or date-time string.</param>
    public CalDateTime(string value)
    {
        var serializer = new DateTimeSerializer();
        CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable
                 ?? throw new InvalidOperationException("Failure deserializing value"));
    }

    private void Initialize(DateTime dateTime, bool hasTime, string? tzId, Calendar? cal)
    {
        DateTime initialValue;

        if ((tzId != null && !string.IsNullOrWhiteSpace(tzId) && !tzId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            || (string.IsNullOrWhiteSpace(tzId) && dateTime.Kind == DateTimeKind.Local))
        {
            // Definitely local
            _tzId = tzId;

            initialValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

        }
        else if (string.Equals("UTC", tzId, StringComparison.OrdinalIgnoreCase) || dateTime.Kind == DateTimeKind.Utc)
        {
            // It is UTC
            _tzId = "UTC";

            initialValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
        else
        {
            // Unspecified
            _tzId = null;

            initialValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }

        SynchronizeDateTimeFields(initialValue, hasTime);

        AssociatedObject = cal;
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
        base.CopyFrom(obj);

        if (obj is not IDateTime dt)
        {
            return;
        }

        if (dt is CalDateTime calDt)
        {
            // Maintain the private date/time backing fields
            _dateOnly = calDt._dateOnly;
            _timeOnly = calDt._timeOnly;

            // Copy the underlying DateTime value and time zone
            _value = calDt._value;
            _tzId = calDt._tzId;
        }

        AssociateWith(dt);
    }

    public bool Equals(CalDateTime other)
        => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is IDateTime && (CalDateTime)obj == this;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Value.GetHashCode();
            hashCode = (hashCode * 397) ^ HasDate.GetHashCode();
            hashCode = (hashCode * 397) ^ AsUtc.GetHashCode();
            hashCode = (hashCode * 397) ^ (TzId != null ? TzId.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator <(CalDateTime? left, IDateTime? right)
        => left != null && right != null && left.AsUtc < right.AsUtc;

    public static bool operator >(CalDateTime? left, IDateTime? right)
        => left != null && right != null && left.AsUtc > right.AsUtc;

    public static bool operator <=(CalDateTime? left, IDateTime? right)
        => left != null && right != null && left.AsUtc <= right.AsUtc;

    public static bool operator >=(CalDateTime? left, IDateTime? right)
        => left != null && right != null && left.AsUtc >= right.AsUtc;

    public static bool operator ==(CalDateTime? left, IDateTime? right)
    {
        return ReferenceEquals(left, null) || ReferenceEquals(right, null)
            ? ReferenceEquals(left, right)
            : right is CalDateTime calDateTime
                && left.Value.Equals(calDateTime.Value)
                && left.HasDate == calDateTime.HasDate
                && left.HasTime == calDateTime.HasTime
                && left.AsUtc.Equals(calDateTime.AsUtc)
                && string.Equals(left.TzId, calDateTime.TzId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(CalDateTime? left, IDateTime? right)
        => !(left == right);

    /// <summary>
    /// Subtracts a <see cref="TimeSpan"/> from the <see cref="CalDateTime"/>.
    /// </summary>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the <see cref="TimeSpan"/> is not a multiple of 24 hours.
    /// </remarks>
    public static IDateTime operator -(CalDateTime left, TimeSpan right)
    {
        var copy = left.Copy<CalDateTime>();
        if (Math.Abs(right.TotalDays % 1) > AlmostZeroEpsilon)
        {
            copy.HasTime = true;
        }
        copy.Value -= right;
        return copy;
    }

    /// <summary>
    /// Adds a <see cref="TimeSpan"/> to the <see cref="CalDateTime"/>.
    /// </summary>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the <see cref="TimeSpan"/> is not a multiple of 24 hours.
    /// </remarks>
    public static IDateTime operator +(CalDateTime left, TimeSpan right)
    {
        var copy = left.Copy<CalDateTime>();
        if (Math.Abs(right.TotalDays % 1) > AlmostZeroEpsilon)
        {
            copy.HasTime = true;
        }
        copy.Value += right;
        return copy;
    }

    /// <summary>
    /// Creates a new instance of <see cref="CalDateTime"/> with <see langword="true"/> for <see cref="HasTime"/>
    /// </summary>
    public static implicit operator CalDateTime(DateTime left) => new CalDateTime(left);

    /// <summary>
    /// Converts the date/time to the date/time of the computer running the program.
    /// If the DateTimeKind is Unspecified, it's assumed that the underlying
    /// Value already represents the system's datetime.
    /// </summary>
    public DateTime AsSystemLocal => AsDateTimeOffset.LocalDateTime;

    /// <summary>
    /// Returns a representation of the <see cref="DateTime"/> in UTC.
    /// </summary>
    public DateTime AsUtc => AsDateTimeOffset.UtcDateTime;

    /// <summary>
    /// Gets the underlying <see cref="DateOnlyValue"/> of <see cref="Value"/>.
    /// </summary>
    public DateOnly? DateOnlyValue => _dateOnly;

    /// <summary>
    /// Gets the underlying <see cref="TimeOnlyValue"/> of <see cref="Value"/>.
    /// </summary>
    public TimeOnly? TimeOnlyValue => _timeOnly;

    /// <summary>
    /// Gets the underlying <see cref="DateTime"/> <see cref="Value"/>.
    /// Depending on <see cref="HasTime"/> setting,
    /// the <see cref="DateTime"/> returned has <see cref="DateTime.TimeOfDay"/>
    /// set to midnight or the time from initialization. The precision of the time part is up to seconds.
    /// <para/>
    /// See also <seealso cref="DateOnlyValue"/> and <seealso cref="TimeOnlyValue"/> for the date and time parts.
    /// </summary>
    public DateTime Value
    {
        get
        {
            // HasDate and HasTime both have setters, so they can be changed.
            if (_dateOnly.HasValue && _timeOnly.HasValue)
            {
                return new DateTime(_dateOnly.Value.Year, _dateOnly.Value.Month,
                    _dateOnly.Value.Day, _timeOnly.Value.Hour, _timeOnly.Value.Minute, _timeOnly.Value.Second,
                    _value.Kind);
            }

            if (_dateOnly.HasValue) // _timeOnly is null here
                return new DateTime(_dateOnly.Value.Year, _dateOnly.Value.Month, _dateOnly.Value.Day,
                    0, 0, 0,
                    _value.Kind);

            throw new InvalidOperationException($"Cannot create DateTime when {nameof(HasDate)} is false.");
        }

        set
        {
            // Kind must be checked in addition to the value,
            // as the value can be the same but the Kind different.
            if (_value == value && _value.Kind == value.Kind)
            {
                return;
            }

            // Initialize with the new value, keeping current 'HasTime' setting
            Initialize(value, _timeOnly.HasValue, TzId, Calendar);
        }
    }

    /// <summary>
    /// Returns true if the underlying <see cref="DateTime"/> <see cref="Value"/> is in UTC.
    /// </summary>
    public bool IsUtc => _value.Kind == DateTimeKind.Utc;

    /// <summary>
    /// Toggles the <see cref="DateTime.TimeOfDay"/> part of the underlying <see cref="Value"/>.
    /// <see langword="true"/> if the underlying <see cref="DateTime"/> <see cref="Value"/> has a 'date' part (year, month, day).
    /// </summary>
    public bool HasDate
    {
        get => _dateOnly.HasValue;
        set => _dateOnly = value ? DateOnly.FromDateTime(_value) : null;
    }

    /// <summary>
    /// Toggles the <see cref="DateTime.TimeOfDay"/> part of the underlying <see cref="Value"/>.
    /// <see langword="true"/> if the underlying <see cref="DateTime"/> <see cref="Value"/> has a 'time' part (hour, minute, second).
    /// </summary>
    public bool HasTime
    {
        get => _timeOnly.HasValue;
        set => _timeOnly = value ? TimeOnly.FromDateTime(_value) : null;
    }

    private string? _tzId = string.Empty;

    /// <summary>
    /// Setting the <see cref="TzId"/> to a local time zone will set <see cref="Value"/> to <see cref="DateTimeKind.Local"/>.
    /// Setting <see cref="TzId"/> to UTC will set <see cref="Value"/> to <see cref="DateTimeKind.Utc"/>.
    /// If the value is set to <see langword="null"/>  or whitespace, <see cref="Value"/> will be <see cref="DateTimeKind.Unspecified"/>.
    /// <para/>
    /// Setting the <see cref="TzId"/> will initialize in the same way aw with the <seealso cref="CalDateTime(DateTime, string, bool)"/>.<br/>
    /// To convert to another time zone, use <see cref="ToTimeZone"/>.
    /// </summary>
    public string? TzId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_tzId))
            {
                _tzId = Parameters.Get("TZID");
            }
            return _tzId;
        }
        set
        {
            if (string.Equals(_tzId, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                Initialize(_value, _timeOnly.HasValue, value, Calendar);
                Parameters.Remove("TZID");
                return;
            }

            Initialize(_value, _timeOnly.HasValue, value, Calendar);
            Parameters.Set("TZID", _tzId); // Use the value after the initialization
        }
    }

    /// <summary>
    /// Gets the time zone name, if it references a time zone.
    /// This is an alias for <see cref="TzId"/>.
    /// </summary>
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

    /// <inheritdoc cref="DateTime.Millisecond"/>
    public int Millisecond => Value.Millisecond;

    /// <inheritdoc cref="DateTime.Ticks"/>
    public long Ticks => Value.Ticks;

    /// <inheritdoc cref="DateTime.DayOfWeek"/>
    public DayOfWeek DayOfWeek => Value.DayOfWeek;

    /// <inheritdoc cref="DateTime.DayOfYear"/>
    public int DayOfYear => Value.DayOfYear;

    /// <inheritdoc cref="DateTime.Date"/>
    public DateTime Date => Value.Date;

    /// <inheritdoc cref="DateTime.TimeOfDay"/>
    public TimeSpan TimeOfDay => Value.TimeOfDay;

    /// <summary>
    /// Returns a representation of the <see cref="IDateTime"/> in the <paramref name="tzId"/> time zone
    /// </summary>
    public IDateTime ToTimeZone(string? tzId)
    {
        // If TzId is empty, it's a system-local datetime, so we should use the system time zone as the starting point.
        var originalTzId = string.IsNullOrWhiteSpace(TzId)
            ? TimeZoneInfo.Local.Id
            : TzId;

        var zonedOriginal = DateUtil.ToZonedDateTimeLeniently(Value, originalTzId);
        var converted = zonedOriginal.WithZone(DateUtil.GetZone(tzId));

        return converted.Zone == DateTimeZone.Utc
            ? new CalDateTime(converted.ToDateTimeUtc(), tzId)
            : new CalDateTime(DateTime.SpecifyKind(converted.ToDateTimeUnspecified(), DateTimeKind.Local), tzId);
    }

    /// <summary>
    /// Returns a <see cref="DateTimeOffset"/> representation of the <see cref="Value"/>.
    /// If a TzId is specified, it will use that time zone's UTC offset, otherwise it will use the
    /// system-local time zone.
    /// </summary>
    public DateTimeOffset AsDateTimeOffset =>
        string.IsNullOrWhiteSpace(TzId)
            ? new DateTimeOffset(Value)
            : DateUtil.ToZonedDateTimeLeniently(Value, TzId).ToDateTimeOffset();

    /// <inheritdoc cref="DateTime.Add"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the hours are not a multiple of 24.
    /// </remarks>
    public IDateTime Add(TimeSpan ts) => this + ts;

    /// <summary>Returns a new <see cref="TimeSpan" /> from subtracting the specified <see cref="IDateTime"/> from to the value of this instance.</summary>
    /// <param name="dt"></param>
    public TimeSpan Subtract(IDateTime dt) => (AsUtc - dt.AsUtc)!;

    /// <summary>Returns a new <see cref="IDateTime"/> by subtracting the specified <see cref="TimeSpan" /> from the value of this instance.</summary>
    /// <param name="ts">A interval.</param>
    /// <returns>An object whose value is the difference of the date and time represented by this instance and the time interval represented by <paramref name="ts" />.</returns>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the hours are not a multiple of 24.
    /// </remarks>
    public IDateTime Subtract(TimeSpan ts) => this - ts;

    [Obsolete("This operator will be removed in a future version.", true)]
    public static TimeSpan? operator -(CalDateTime? left, IDateTime? right)
    {
        left?.AssociateWith(right); // Should not be done in operator overloads
        return left?.AsUtc - right?.AsUtc;
    }

    /// <inheritdoc cref="DateTime.AddYears"/>
    public IDateTime AddYears(int years)
    {
        var dt = Copy<CalDateTime>();
        dt.Value = Value.AddYears(years);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddMonths"/>
    public IDateTime AddMonths(int months)
    {
        var dt = Copy<CalDateTime>();
        dt.Value = Value.AddMonths(months);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddDays"/>
    public IDateTime AddDays(int days)
    {
        var dt = Copy<CalDateTime>();
        dt.Value = Value.AddDays(days);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddHours"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the hours are not a multiple of 24.
    /// </remarks>
    public IDateTime AddHours(int hours)
    {
        var dt = Copy<CalDateTime>();
        if (!dt.HasTime && hours % 24 > 0)
        {
            dt.HasTime = true;
        }
        dt.Value = Value.AddHours(hours);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddMinutes"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the minutes are not a multiple of 1440.
    /// </remarks>
    public IDateTime AddMinutes(int minutes)
    {
        var dt = Copy<CalDateTime>();
        if (!dt.HasTime && minutes % 1440 > 0)
        {
            dt.HasTime = true;
        }
        dt.Value = Value.AddMinutes(minutes);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddSeconds"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>,
    /// if the seconds are not a multiple of 86400.
    /// </remarks>
    public IDateTime AddSeconds(int seconds)
    {
        var dt = Copy<CalDateTime>();
        if (!dt.HasTime && seconds % 86400 > 0)
        {
            dt.HasTime = true;
        }
        dt.Value = Value.AddSeconds(seconds);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddMilliseconds"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>
    /// if the milliseconds are not a multiple of 86400000.
    /// <para/>
    /// Milliseconds less than full seconds get truncated.
    /// </remarks>
    public IDateTime AddMilliseconds(int milliseconds)
    {
        var dt = Copy<CalDateTime>();
        if (!dt.HasTime && milliseconds % 86400000 > 0)
        {
            dt.HasTime = true;
        }
        dt.Value = Value.AddMilliseconds(milliseconds);
        return dt;
    }

    /// <inheritdoc cref="DateTime.AddTicks"/>
    /// <remarks>
    /// This will also set <seealso cref="HasTime"/> to <see langword="true"/>.
    /// if ticks do not result in multiple of full days.
    /// <para/>
    /// Ticks less than full seconds get truncated.
    /// </remarks>
    public IDateTime AddTicks(long ticks)
    {
        var dt = Copy<CalDateTime>();
        if (!dt.HasTime && Math.Abs(TimeSpan.FromTicks(ticks).TotalDays % 1) > AlmostZeroEpsilon)
        {
            dt.HasTime = true;
        }

        dt.Value = Value.AddTicks(ticks);
        return dt;
    }

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
        var dateTimeOffset = AsDateTimeOffset;

        // Use the .NET format options to format the DateTimeOffset

        if (HasTime && !HasDate)
        {
            return $"{dateTimeOffset.TimeOfDay.ToString(format, formatProvider)} {_tzId}";
        }

        if (HasTime && HasDate)
        {
            return $"{dateTimeOffset.ToString(format, formatProvider)} {_tzId}";
        }

        return $"{dateTimeOffset.ToString("d", formatProvider)} {_tzId}";
    }

    private void SynchronizeDateTimeFields(DateTime dateTime, bool hasTime)
    {
        _value = dateTime;
        _dateOnly = DateOnly.FromDateTime(_value);
        _timeOnly = hasTime ? TimeOnly.FromDateTime(_value) : null;
    }
}
