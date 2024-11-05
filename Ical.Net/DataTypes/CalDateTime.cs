#nullable enable
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using System;
using System.Globalization;
using System.IO;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="CalDateTime"/>
    /// class handles time zones, and integrates seamlessly into the iCalendar framework.
    /// </remarks>
    /// </summary>
    public sealed class CalDateTime : EncodableDataType, IDateTime
    {
        public static CalDateTime Now => new CalDateTime(DateTime.Now);

        public static CalDateTime Today => new CalDateTime(DateTime.Today);

        public static CalDateTime UtcNow => new CalDateTime(DateTime.UtcNow);

        private DateOnly? _dateOnly;
        private TimeOnly? _timeOnly;

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
            if (value.HasTime)
                Initialize(new DateOnly(value.Year, value.Month, value.Day), new TimeOnly(value.Hour, value.Minute, value.Second), value.Date.Kind, value.TzId, null);
            else
                Initialize(new DateOnly(value.Year, value.Month, value.Day), null, value.Date.Kind, value.TzId, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class
        /// and sets the <see cref="TzId"/> to "UTC" if the <paramref name="value"/>
        /// has a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="value"></param>
        public CalDateTime(DateTime value) : this(value, value.Kind == DateTimeKind.Utc ? "UTC" : null)
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
        public CalDateTime(DateTime value, string? tzId)
        {
            Initialize(new DateOnly(value.Year, value.Month, value.Day), new TimeOnly(value.Hour, value.Minute, value.Second), value.Date.Kind, tzId, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class
        /// with the <see cref="Value"/>'s <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        public CalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, null, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
        /// </summary>
        /// <param name="second"></param>
        /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
        /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
        /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Unspecified"/>.
        /// </param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId)
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, tzId, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
        /// </summary>
        /// <param name="second"></param>
        /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
        /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
        /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Unspecified"/>.
        /// </param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="cal"></param>
        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string? tzId, Calendar cal) //NOSONAR - must keep this signature
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, tzId, cal);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class
        /// with the <see cref="Value"/>'s <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        public CalDateTime(int year, int month, int day)
        {
            Initialize(new DateOnly(year, month, day), null, DateTimeKind.Unspecified, null, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
        /// </summary>
        /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
        /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
        /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Unspecified"/>.
        /// </param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        public CalDateTime(int year, int month, int day, string tzId)
        {
            Initialize(new DateOnly(year, month, day), null, DateTimeKind.Unspecified, tzId, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalDateTime"/> class using the specified time zone.
        /// </summary>
        /// <param name="tzId">The specified value will override value's <see cref="DateTime.Kind"/> property.
        /// If a non-UTC time zone is specified, the underlying  <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Local"/>.
        /// If no time zone is specified, the <see cref="DateTime.Kind"/> property will be <see cref="DateTimeKind.Unspecified"/>.
        /// </param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        public CalDateTime(DateOnly date, TimeOnly time, string? tzId = null)
        {
            Initialize(date, time, DateTimeKind.Unspecified, tzId, null);
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

        /// <summary>
        /// Sets the <see cref="DateTime"/> <see cref="Value"/> so that it
        /// can be used as a date-only or a date-time value.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <param name="kind"></param>
        public void SetValue(DateOnly date, TimeOnly? time, DateTimeKind kind)
        {
            _value = time.HasValue
                ? new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, kind)
                : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, kind);

            _dateOnly = date;
            _timeOnly = time;
        }

        private void Initialize(DateOnly date, TimeOnly? time, DateTimeKind kind, string? tzId, Calendar? cal)
        {
            _dateOnly = date;
            _timeOnly = time;

            if ((tzId != null && !string.IsNullOrWhiteSpace(tzId) && !tzId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                || (string.IsNullOrWhiteSpace(tzId) && kind == DateTimeKind.Local))
            {
                // Definitely local
                TzId = tzId;

                _value = time.HasValue
                    ? new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, DateTimeKind.Local)
                    : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
            }
            else if (string.Equals("UTC", tzId, StringComparison.OrdinalIgnoreCase) || kind == DateTimeKind.Utc)
            {
                // It is UTC
                TzId = "UTC";

                _value = time.HasValue
                    ? new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, DateTimeKind.Utc)
                    : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                // Unspecified
                TzId = null;

                _value = time.HasValue
                    ? new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, kind)
                    : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, kind);
            }

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
                // Maintain the private date/time only flags form the original object
                _dateOnly = calDt._dateOnly;
                _timeOnly = calDt._timeOnly;
            }

            // Copy the underlying DateTime value and time zone ID
            _value = dt.Value;
            TzId = dt.TzId;

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
                : right is CalDateTime
                    && left.Value.Equals(right.Value)
                    && left.HasDate == right.HasDate
                    && left.AsUtc.Equals(right.AsUtc)
                    && string.Equals(left.TzId, right.TzId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(CalDateTime? left, IDateTime? right)
            => !(left == right);

        public static TimeSpan? operator -(CalDateTime? left, IDateTime? right)
        {
            left?.AssociateWith(right);
            return left?.AsUtc - right?.AsUtc;
        }

        public static IDateTime operator -(CalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value -= right;
            return copy;
        }

        public static IDateTime operator +(CalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value += right;
            return copy;
        }

        public static implicit operator CalDateTime(DateTime left) => new CalDateTime(left);

        /// <summary>
        /// Converts the date/time to the date/time of the computer running the program. If the DateTimeKind is Unspecified, it's assumed that the underlying
        /// Value already represents the system's datetime.
        /// </summary>
        public DateTime AsSystemLocal
        {
            get
            {
                if (Value.Kind == DateTimeKind.Unspecified)
                {
                    return HasTime
                        ? Value
                        : Value.Date;
                }

                return HasTime
                    ? Value.ToLocalTime()
                    : Value.ToLocalTime().Date;
            }
        }

        /// <summary>
        /// Returns a representation of the <see cref="DateTime"/> in UTC.
        /// </summary>
        public DateTime AsUtc
        {
            get
            {
                // In order of weighting:
                //  1) Specified TzId
                //  2) Value having a DateTimeKind.Utc
                //  3) Use the OS's time zone
                DateTime asUtc;

                if (!string.IsNullOrWhiteSpace(TzId))
                {
                    var asLocal = DateUtil.ToZonedDateTimeLeniently(Value, TzId);
                    return asLocal.ToDateTimeUtc();
                }

                if (IsUtc || Value.Kind == DateTimeKind.Utc)
                {
                    asUtc = DateTime.SpecifyKind(Value, DateTimeKind.Utc);
                    return asUtc;
                }

                asUtc = DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();
                return asUtc;
            }
        }

        private DateTime _value;

        /// <summary>
        /// Gets the underlying <see cref="DateTime"/> <see cref="Value"/>.
        /// Use <see cref="SetValue"/> for setting the value.
        /// </summary>
        public DateTime Value
        {
            get => _value;

            [Obsolete("This setter is depreciated and will be removed in a future version. Use SetValue instead.", false)]
            set
            {
                // Kind must be checked in addition to the value,
                // as the value can be the same but the Kind different.
                if (_value == value && _value.Kind == value.Kind)
                {
                    return;
                }

                // Maintain the initial date/time only flags
                // if the date/time parts are unchanged.
                // This is a temporary workaround.

                if (value.Date != _value.Date)
                    _dateOnly = DateOnly.FromDateTime(value);

                if (value.TimeOfDay != _value.TimeOfDay)
                    _timeOnly = TimeOnly.FromDateTime(value);

                _value = value;
            }
        }

        /// <summary>
        /// Returns true if the underlying <see cref="DateTime"/> <see cref="Value"/> is in UTC.
        /// </summary>
        public bool IsUtc => _value.Kind == DateTimeKind.Utc;

        /// <summary>
        /// Returns <see langword="true"/> if the underlying <see cref="DateTime"/> <see cref="Value"/> has a 'date' part (year, month, day).
        /// </summary>
        public bool HasDate
        {
            get => _dateOnly.HasValue;
            [Obsolete("This setter is depreciated and will be removed in a future version. Use SetValue instead.", false)]
            set
            {
                if (value && !_dateOnly.HasValue) _dateOnly = new DateOnly(Value.Year, Value.Month, Value.Day);
                if (!value) _dateOnly = null;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the underlying <see cref="DateTime"/> <see cref="Value"/> has a 'time' part (hour, minute, second).
        /// </summary>
        public bool HasTime
        {
            get => _timeOnly.HasValue;
            [Obsolete("This setter is depreciated and will be removed in a future version. Use SetValue instead.", false)]
            set
            {
                if (value && !_timeOnly.HasValue) _timeOnly = new TimeOnly(Value.Hour, Value.Minute, Value.Hour);
                if (!value) _timeOnly = null;
            }
        }

        private string? _tzId = string.Empty;

        /// <summary>
        /// Setting the <see cref="TzId"/> to a local time zone will set <see cref="Value"/> to <see cref="DateTimeKind.Local"/>.
        /// Setting <see cref="TzId"/> to UTC will set <see cref="Value"/> to <see cref="DateTimeKind.Utc"/>.
        /// If the value is set to <see langword="null"/>  or whitespace, <see cref="Value"/> will be <see cref="DateTimeKind.Unspecified"/>.
        /// Setting the TzId will NOT incur a UTC offset conversion.
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

                var isEmpty = string.IsNullOrWhiteSpace(value);
                if (isEmpty)
                {
                    Parameters.Remove("TZID");
                    _tzId = null;
                    _value = DateTime.SpecifyKind(_value, DateTimeKind.Local);
                    return;
                }

                var kind = string.Equals(value, "UTC", StringComparison.OrdinalIgnoreCase)
                    ? DateTimeKind.Utc
                    : DateTimeKind.Local;

                _value = DateTime.SpecifyKind(_value, kind);
                Parameters.Set("TZID", value);
                _tzId = value;
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
                ? new DateTimeOffset(AsSystemLocal)
                : DateUtil.ToZonedDateTimeLeniently(Value, TzId).ToDateTimeOffset();

        /// <inheritdoc cref="DateTime.Add"/>
        public IDateTime Add(TimeSpan ts) => this + ts;

        public IDateTime Subtract(TimeSpan ts) => this - ts;

        public TimeSpan Subtract(IDateTime dt) => (TimeSpan)(this - dt)!;

        /// <inheritdoc cref="DateTime.AddYears"/>
        public IDateTime AddYears(int years)
        {
            var dt = Copy<IDateTime>();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddMonths"/>
        public IDateTime AddMonths(int months)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddMonths(months);
            dt.SetValue(DateOnly.FromDateTime(newValue), dt._timeOnly, newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddDays"/>
        public IDateTime AddDays(int days)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddDays(days);
            dt.SetValue(DateOnly.FromDateTime(newValue), dt._timeOnly, newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddHours"/>
        public IDateTime AddHours(int hours)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddHours(hours);
            dt.SetValue(DateOnly.FromDateTime(newValue), TimeOnly.FromDateTime(newValue), newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddMinutes"/>
        public IDateTime AddMinutes(int minutes)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddMinutes(minutes);
            dt.SetValue(DateOnly.FromDateTime(newValue), TimeOnly.FromDateTime(newValue), newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddSeconds"/>
        public IDateTime AddSeconds(int seconds)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddSeconds(seconds);
            dt.SetValue(DateOnly.FromDateTime(newValue), TimeOnly.FromDateTime(newValue), newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddMilliseconds"/>
        /// <remarks>Milliseconds less than full seconds get truncated.</remarks>
        public IDateTime AddMilliseconds(int milliseconds)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddMilliseconds(milliseconds);
            dt.SetValue(DateOnly.FromDateTime(newValue), TimeOnly.FromDateTime(newValue), newValue.Kind);
            return dt;
        }

        /// <inheritdoc cref="DateTime.AddTicks"/>
        /// <remarks>Ticks less than full seconds get truncated.</remarks>
        public IDateTime AddTicks(long ticks)
        {
            var dt = Copy<CalDateTime>();
            var newValue = Value.AddTicks(ticks);
            dt.SetValue(DateOnly.FromDateTime(newValue), TimeOnly.FromDateTime(newValue), newValue.Kind);
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
    }
}