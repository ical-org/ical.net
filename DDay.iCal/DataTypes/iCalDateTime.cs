using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="iCalDateTime"/>
    /// class handles time zone differences, and integrates seamlessly into
    /// the iCalendar framework.
    /// </remarks>
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalDateTime", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public sealed class iCalDateTime :
        EncodableDataType,
        IDateTime        
    {
        #region Private Fields

        private DateTime _Value;
        private bool _HasDate;
        private bool _HasTime;
        private ITimeZoneInfo _TimeZoneInfo;
        private bool _IsUniversalTime;
        private string _TZID;        

        #endregion

        #region Constructors

        public iCalDateTime() { }
        public iCalDateTime(IDateTime value)
        {
            Initialize(value.Value, value.TZID);
        }
        public iCalDateTime(DateTime value) : this(value, null) {}
        public iCalDateTime(DateTime value, string tzid) 
        {
            Initialize(value, tzid);
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(year, month, day, hour, minute, second, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid)
        {
            Initialize(year, month, day, hour, minute, second, tzid);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, string tzid)
            : this(year, month, day, 0, 0, 0, tzid) { }

        private void Initialize(int year, int month, int day, int hour, int minute, int second, string tzid)
        {
            Initialize(CoerceDateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid);            
        }

        private void Initialize(DateTime value, string tzid)
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            // Convert all incoming values to UTC.
            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            this.TZID = tzid;            
        }

        private DateTime CoerceDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            DateTime dt = DateTime.MinValue;

            // NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
            // If so, let's automatically adjust the date/time to compensate.
            // FIXME: should we have a parsing setting that will throw an exception
            // instead of automatically adjusting the date/time value to the
            // closest representable date/time?
            try
            {
                if (year > 9999)
                    dt = DateTime.MaxValue;
                else if (year > 0)
                    dt = new DateTime(year, month, day, hour, minute, second, kind);                
            }
            catch
            {                
            }

            return dt;
        }
        
        #endregion

        #region Protected Methods

        protected ITimeZoneInfo GetTimeZoneInfo()
        {
            if (_TimeZoneInfo == null && TZID != null && Calendar != null)
            {
                ITimeZone tz = Calendar.GetTimeZone(TZID);
                if (tz != null)
                    _TimeZoneInfo = tz.GetTimeZoneInfo(this);
            }
            return _TimeZoneInfo;
        }

        #endregion

        #region Overrides

        public override void AssociateWith(ICalendarObject obj)
        {
            if (!object.Equals(AssociatedObject, obj))
            {                
                base.AssociateWith(obj);
                
                _TimeZoneInfo = null;                
            }            
        }        

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            IDateTime dt = obj as IDateTime;
            if (dt != null)
            {
                Value = dt.Value;
                IsUniversalTime = dt.IsUniversalTime;
                TZID = dt.TZID;
                HasDate = dt.HasDate;
                HasTime = dt.HasTime;
                _TimeZoneInfo = null;
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj is IDateTime)
            {
                Associate(this, (IDateTime)obj);
                return ((IDateTime)obj).UTC.Equals(UTC);
            }
            else if (obj is DateTime)
            {
                iCalDateTime dt = (iCalDateTime)obj;
                Associate(this, dt);
                return object.Equals(dt.UTC, UTC);
            }
            return false;            
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {            
            string tz = " " + TimeZoneName;

            if (HasTime && HasDate)
                return Value.ToString() + tz;
            else if (HasTime)
                return Value.TimeOfDay.ToString() + tz;
            else
                return Value.ToShortDateString() + tz;
        }

        #endregion

        #region Operators

        static private void Associate(IDateTime left, IDateTime right)
        {
            if (left.AssociatedObject == null && right.AssociatedObject != null)
                left.AssociateWith(right.AssociatedObject);
            else if (left.AssociatedObject != null && right.AssociatedObject == null)
                right.AssociateWith(left.AssociatedObject);
        }

        public static bool operator <(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return left.UTC < right.UTC;
            else return left.UTC.Date < right.UTC.Date;
        }

        public static bool operator >(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return left.UTC > right.UTC;
            else return left.UTC.Date > right.UTC.Date;
        }

        public static bool operator <=(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return left.UTC <= right.UTC;
            else return left.UTC.Date <= right.UTC.Date;
        }

        public static bool operator >=(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return left.UTC >= right.UTC;
            else return left.UTC.Date >= right.UTC.Date;
        }

        public static bool operator ==(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return left.UTC.Equals(right.UTC);
            else return left.UTC.Date.Equals(right.UTC.Date);
        }

        public static bool operator !=(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);

            if (left.HasTime || right.HasTime)
                return !left.UTC.Equals(right.UTC);
            else return !left.UTC.Date.Equals(right.UTC.Date);
        }

        public static TimeSpan operator -(iCalDateTime left, IDateTime right)
        {
            Associate(left, right);
            return left.UTC - right.UTC;
        }

        public static IDateTime operator -(iCalDateTime left, TimeSpan right)
        {            
            IDateTime copy = left.Copy<IDateTime>();
            copy.Value -= right;
            return copy;
        }

        public static IDateTime operator +(iCalDateTime left, TimeSpan right)
        {
            IDateTime copy = left.Copy<IDateTime>();
            copy.Value += right;
            return copy;
        }

        public static implicit operator iCalDateTime(DateTime left)
        {
            return new iCalDateTime(left);
        }

        #endregion        

        #region IDateTime Members

        /// <summary>
        /// Converts the date/time to this computer's local date/time.
        /// </summary>
        public DateTime Local
        {
            get
            {
                if (!HasTime)
                    return DateTime.SpecifyKind(Value.Date, DateTimeKind.Local);
                else if (IsUniversalTime)
                    return Value.ToLocalTime();
                else
                    return UTC.ToLocalTime();
            }
        }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        public DateTime UTC
        {
            get
            {
                if (IsUniversalTime)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Utc);
                else
                {
                    DateTime value = Value;
                    ITimeZoneInfo tzi = TimeZoneInfo;
                    if (tzi != null)
                    {
                        int mult = tzi.OffsetTo.Positive ? -1 : 1;
                        value = value.AddHours(tzi.OffsetTo.Hours * mult);
                        value = value.AddMinutes(tzi.OffsetTo.Minutes * mult);
                        value = value.AddSeconds(tzi.OffsetTo.Seconds * mult);
                        value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                        return value;
                    }
                    else
                    {
                        // Fallback to the OS-conversion
                        value = DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();
                    }
                    return value;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public ITimeZoneInfo TimeZoneInfo
        {
            get
            {
                return GetTimeZoneInfo();
            }
        }

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public bool IsUniversalTime
        {
            get { return _IsUniversalTime; }
            set { _IsUniversalTime = value; }
        }

        public string TimeZoneName
        {
            get
            {
                if (IsUniversalTime)
                    return "UTC";
                else if (_TimeZoneInfo != null)
                    return _TimeZoneInfo.TimeZoneName;
                return string.Empty;
            }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public DateTime Value
        {
            get { return _Value; }
            set
            {
                if (!object.Equals(_Value, value))
                {
                    _Value = value;

                    // Reset the time zone info, since we may be in
                    // a different time zone now that we have a different
                    // date/time value (i.e. STANDARD vs DAYLIGHT, etc.).
                    _TimeZoneInfo = null;
                }
                    
            }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public bool HasDate
        {
            get { return _HasDate; }
            set { _HasDate = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public bool HasTime
        {
            get { return _HasTime; }
            set { _HasTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        public string TZID
        {
            get { return _TZID; }
            set
            {
                if (!object.Equals(_TZID, value))
                {
                    _TZID = value;

                    _TimeZoneInfo = null;                    
                }
            }
        }

        public int Year
        {
            get { return Value.Year; }
        }

        public int Month
        {
            get { return Value.Month; }
        }

        public int Day
        {
            get { return Value.Day; }
        }

        public int Hour
        {
            get { return Value.Hour; }
        }

        public int Minute
        {
            get { return Value.Minute; }
        }

        public int Second
        {
            get { return Value.Second; }
        }

        public int Millisecond
        {
            get { return Value.Millisecond; }
        }

        public long Ticks
        {
            get { return Value.Ticks; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return Value.DayOfWeek; }
        }

        public int DayOfYear
        {
            get { return Value.DayOfYear; }
        }

        public DateTime Date
        {
            get { return Value.Date; }
        }

        public TimeSpan TimeOfDay
        {
            get { return Value.TimeOfDay; }
        }        

        public DateTime ToTimeZone(ITimeZoneInfo tzi)
        {
            DateTime value = UTC;

            int mult = tzi.OffsetTo.Positive ? 1 : -1;
            value = value.AddHours(tzi.OffsetTo.Hours * mult);
            value = value.AddMinutes(tzi.OffsetTo.Minutes * mult);
            value = value.AddSeconds(tzi.OffsetTo.Seconds * mult);
            value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            return value;
        }

        public DateTime ToTimeZone(string tzid)
        {
            if (tzid != null)
            {
                if (Calendar != null)
                {
                    ITimeZone tz = Calendar.GetTimeZone(tzid);
                    if (tz != null)
                    {
                        ITimeZoneInfo tzi = tz.GetTimeZoneInfo(this);
                        if (tzi != null)
                            return ToTimeZone(tzi);
                    }
                    throw new Exception("The '" + tzid + "' time zone could not be resolved.");
                }
                else throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
            }
            else throw new ArgumentException("You must provide a valid TZID to the ToTimeZone() method", "tzid");
        }

        public IDateTime Add(TimeSpan ts)
        {
            return this + ts;
        }

        public IDateTime Subtract(TimeSpan ts)
        {
            return this - ts;
        }

        public TimeSpan Subtract(IDateTime dt)
        {
            return this - dt;
        }

        public IDateTime AddYears(int years)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        public IDateTime AddMonths(int months)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddMonths(months);
            return dt;
        }

        public IDateTime AddDays(int days)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddDays(days);
            return dt;
        }

        public IDateTime AddHours(int hours)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public IDateTime AddMinutes(int minutes)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public IDateTime AddSeconds(int seconds)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public IDateTime AddMilliseconds(int milliseconds)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
        }

        public bool LessThan(IDateTime dt)
        {
            return this < dt;
        }

        public bool GreaterThan(IDateTime dt)
        {
            return this > dt;
        }

        public bool LessThanOrEqual(IDateTime dt)
        {
            return this <= dt;
        }

        public bool GreaterThanOrEqual(IDateTime dt)
        {
            return this >= dt;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(IDateTime dt)
        {
            if (this.Equals(dt))
                return 0;
            else if (this < dt)
                return -1;
            else if (this > dt)
                return 1;
            throw new Exception("An error occurred while comparing two IDateTime values.");
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }

        #endregion
    }
}
