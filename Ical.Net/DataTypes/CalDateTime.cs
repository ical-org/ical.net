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

        public CalDateTime(IDateTime value)
        {
            if (value.HasTime)
                Initialize(new DateOnly(value.Year, value.Month, value.Day), new TimeOnly(value.Hour, value.Minute, value.Second), value.Date.Kind, value.TzId, null);
            else
                Initialize(new DateOnly(value.Year, value.Month, value.Day), null, value.Date.Kind, value.TzId, null);
        }

        public CalDateTime(DateTime value) : this(value, value.Kind == DateTimeKind.Utc ? "UTC" : null)
        { }

        /// <summary>
        /// Specifying a `tzId` value will override `value`'s `DateTimeKind` property. If the time zone specified is UTC, the underlying `DateTimeKind` will be
        /// `Utc`. If a non-UTC time zone is specified, the underlying `DateTimeKind` property will be `Local`. If no time zone is specified, the `DateTimeKind`
        /// property will be left untouched.
        /// </summary>
        public CalDateTime(DateTime value, string tzId)
        {
            Initialize(new DateOnly(value.Year, value.Month, value.Day), new TimeOnly(value.Hour, value.Minute, value.Second), value.Date.Kind, tzId, null);
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, null, null);
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId)
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, tzId, null);
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId, Calendar cal)
        {
            Initialize(new DateOnly(year, month, day), new TimeOnly(hour, minute, second), DateTimeKind.Unspecified, tzId, cal);
        }

        public CalDateTime(int year, int month, int day)
        {
            Initialize(new DateOnly(year, month, day), null, DateTimeKind.Unspecified, null, null);
        }

        public CalDateTime(int year, int month, int day, string tzId)
        {
            Initialize(new DateOnly(year, month, day), null, DateTimeKind.Unspecified, tzId, null);
        }

        public CalDateTime(DateOnly date, TimeOnly time, string tzId = null)
        {
            Initialize(date, time, DateTimeKind.Unspecified, tzId, null);
        }

        public CalDateTime(string value)
        {
            var serializer = new DateTimeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        /// <summary>
        /// Sets the date/time for <see cref="Value"/> so that it
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

        private void Initialize(DateOnly date, TimeOnly? time, DateTimeKind kind, string tzId, Calendar cal)
        {
            _dateOnly = date;
            _timeOnly = time;

            if ((!string.IsNullOrWhiteSpace(tzId) && !tzId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                || (string.IsNullOrEmpty(tzId) && kind == DateTimeKind.Local))
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
                TzId = string.Empty;

                _value = time.HasValue
                    ? new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, DateTimeKind.Unspecified)
                    : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }

            AssociatedObject = cal;
        }

        public override ICalendarObject AssociatedObject
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

        public override bool Equals(object? other)
            => other is IDateTime && (CalDateTime)other == this;

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

        public static bool operator <(CalDateTime left, IDateTime right)
            => left != null && right != null && left.AsUtc < right.AsUtc;

        public static bool operator >(CalDateTime left, IDateTime right)
            => left != null && right != null && left.AsUtc > right.AsUtc;

        public static bool operator <=(CalDateTime left, IDateTime right)
            => left != null && right != null && left.AsUtc <= right.AsUtc;

        public static bool operator >=(CalDateTime left, IDateTime right)
            => left != null && right != null && left.AsUtc >= right.AsUtc;

        public static bool operator ==(CalDateTime left, IDateTime right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null)
                ? ReferenceEquals(left, right)
                : right is CalDateTime
                    && left.Value.Equals(right.Value)
                    && left.HasDate == right.HasDate
                    && left.AsUtc.Equals(right.AsUtc)
                    && string.Equals(left.TzId, right.TzId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(CalDateTime left, IDateTime right)
            => !(left == right);

        public static TimeSpan operator -(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);
            return left.AsUtc - right.AsUtc;
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
        /// Returns a representation of the DateTime in Coordinated Universal Time (UTC)
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
        /// Gets/sets the underlying DateTime value stored.
        /// Use <see cref="SetValue"/> instead.
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
        /// Returns true if the underlying DateTime value is in UTC.
        /// </summary>
        public bool IsUtc => _value.Kind == DateTimeKind.Utc;

        /// <summary>
        /// Returns true if the underlying DateTime has a 'date' part (year, month, day),
        /// otherwise the DateTime is considered a 'time' only.
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
        /// Returns true if the underlying DateTime has a 'time' part (hour, minute, second),
        /// otherwise the DateTime is considered a 'date' only.
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

        private string _tzId = string.Empty;

        /// <summary>
        /// Setting the <see cref="TzId"/> to a local time zone will set <see cref="Value"/> to <see cref="DateTimeKind.Local"/>.
        /// Setting <see cref="TzId"/> to UTC will set <see cref="Value"/> to <see cref="DateTimeKind.Utc"/>.
        /// If the value is set to <see langword="null"/>  or whitespace, <see cref="Value"/> will be <see cref="DateTimeKind.Unspecified"/>.
        /// Setting the TzId will NOT incur a UTC offset conversion.
        /// To convert to another time zone, use <see cref="ToTimeZone"/>.
        /// </summary>
        public string TzId
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

        public string TimeZoneName => TzId;

        public int Year => Value.Year;

        public int Month => Value.Month;

        public int Day => Value.Day;

        public int Hour => Value.Hour;

        public int Minute => Value.Minute;

        public int Second => Value.Second;

        public int Millisecond => Value.Millisecond;

        public long Ticks => Value.Ticks;

        public DayOfWeek DayOfWeek => Value.DayOfWeek;

        public int DayOfYear => Value.DayOfYear;

        public DateTime Date => Value.Date;

        public TimeSpan TimeOfDay => Value.TimeOfDay;

        /// <summary>
        /// Returns a representation of the <see cref="IDateTime"/> in the <paramref name="tzId"/> time zone
        /// </summary>
        public IDateTime ToTimeZone(string tzId)
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

        public TimeSpan Subtract(IDateTime dt) => this - dt;

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

        public bool LessThan(IDateTime dt) => this < dt;

        public bool GreaterThan(IDateTime dt) => this > dt;

        public bool LessThanOrEqual(IDateTime dt) => this <= dt;

        public bool GreaterThanOrEqual(IDateTime dt) => this >= dt;

        public void AssociateWith(IDateTime dt)
        {
            if (AssociatedObject == null && dt.AssociatedObject != null)
            {
                AssociatedObject = dt.AssociatedObject;
            }
            else if (AssociatedObject != null && dt.AssociatedObject == null)
            {
                dt.AssociatedObject = AssociatedObject;
            }
        }

        public int CompareTo(IDateTime dt)
        {
            if (Equals(dt))
            {
                return 0;
            }
            if (this < dt)
            {
                return -1;
            }
            if (this > dt)
            {
                return 1;
            }
            throw new Exception("An error occurred while comparing two IDateTime values.");
        }

        public override string ToString() => ToString(null, null);

        public string ToString(string format) => ToString(format, null);

        public string ToString(string format, IFormatProvider formatProvider)
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