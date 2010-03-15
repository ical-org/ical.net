using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal;
using System.Runtime.Serialization;
using DDay.iCal;

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
    [DebuggerDisplay("{HasTime ? Value.ToString() : Value.ToShortDateString()}")]
#if DATACONTRACT
    [DataContract(Name = "iCalDateTime", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public struct iCalDateTime :
        IComparable,
        IFormattable
    {
        public static readonly iCalDateTime MaxValue;
        public static readonly iCalDateTime MinValue;

        #region Static Constructor

        static iCalDateTime()
        {
            MinValue = default(iCalDateTime);
            MaxValue = new iCalDateTime(DateTime.MaxValue);
        }

        #endregion

        #region Private Fields

        private bool _IsAssigned;
        private DateTime _Value;
        private bool _HasDate;
        private bool _HasTime;
        private string _TZID;
        private ITimeZoneInfo _TimeZoneInfo;
        private bool _IsUniversalTime;
        private IICalendar _Calendar;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public bool IsAssigned
        {
            get { return _IsAssigned; }
            set { _IsAssigned = value; }
        }

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
        /// Retrieves the <see cref="iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public ITimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (_TimeZoneInfo == null && TZID != null)
                {
                    if (Calendar != null)
                    {
                        ITimeZone tz = Calendar.GetTimeZone(TZID);
                        if (tz != null)
                            _TimeZoneInfo = tz.GetTimeZoneInfo(this);
                    }
                    else throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
                }
                return _TimeZoneInfo;
            }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public IICalendar Calendar
        {
            get { return _Calendar; }
            set { _Calendar = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
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
                if (TZID != null &&
                    TimeZoneInfo != null)
                {
                    return TimeZoneInfo.TimeZoneName;
                }
                return string.Empty;
            }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public DateTime Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        public bool HasDate
        {
            get { return _HasDate; }
            set { _HasDate = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        public bool HasTime
        {
            get { return _HasTime; }
            set { _HasTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 7)]
#endif
        public string TZID
        {
            get { return _TZID; }
            set { _TZID = value; }
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

        public iCalDateTime YearDate
        {
            get
            {
                return AddMonths(-Month + 1).AddDays(-Day + 1).AddHours(-Hour + 1).AddMinutes(-Minute + 1).AddSeconds(-Second + 1);
            }
        }

        public iCalDateTime MonthDate
        {
            get
            {
                return AddDays(-Day + 1).AddHours(-Hour + 1).AddMinutes(-Minute + 1).AddSeconds(-Second + 1);
            }
        }

        #endregion

        #region Constructors
                
        public iCalDateTime(iCalDateTime value) : this()
        {
            Initialize(value.Value, value.TZID, value.Calendar);
        }
        public iCalDateTime(DateTime value) : this(value, null, null) {}
        public iCalDateTime(DateTime value, string tzid, IICalendar iCal) : this()
        {
            Initialize(value, tzid, iCal);
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second)
            : this()
        {
            Initialize(year, month, day, hour, minute, second, null, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal)
            : this()
        {
            Initialize(year, month, day, hour, minute, second, tzid, iCal);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, string tzid, IICalendar iCal)
            : this(year, month, day, 0, 0, 0, tzid, iCal) { }

        private void Initialize(int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal)
        {
            Initialize(CoerceDateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid, iCal);            
        }

        private void Initialize(DateTime value, string tzid, IICalendar iCal)
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            // Convert all incoming values to UTC.
            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            this.TZID = tzid;
            this.Calendar = iCal;
            this.IsAssigned = true;
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

        #region Overrides
                
        public void MergeWith(iCalDateTime dt)
        {
            if (Calendar == null)
                Calendar = dt.Calendar;
            if (TZID == null)
                TZID = dt.TZID;
            IsUniversalTime = dt.IsUniversalTime;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is iCalDateTime)
                return ((iCalDateTime)obj).UTC.Equals(UTC);
            else if (obj is DateTime)
                return object.Equals((iCalDateTime)obj, UTC);
            return false;            
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (HasDate)
                return Value.ToString();
            else return Value.ToShortDateString();
        }

        #endregion

        #region Operators

        public static bool operator <(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC < right.UTC;
            else return left.UTC.Date < right.UTC.Date;
        }

        public static bool operator >(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC > right.UTC;
            else return left.UTC.Date > right.UTC.Date;
        }

        public static bool operator <=(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC <= right.UTC;
            else return left.UTC.Date <= right.UTC.Date;
        }

        public static bool operator >=(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC >= right.UTC;
            else return left.UTC.Date >= right.UTC.Date;
        }

        public static bool operator ==(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC.Equals(right.UTC);
            else return left.UTC.Date.Equals(right.UTC.Date);
        }

        public static bool operator !=(iCalDateTime left, iCalDateTime right)
        {
            if (left.HasTime || right.HasTime)
                return !left.UTC.Equals(right.UTC);
            else return !left.UTC.Date.Equals(right.UTC.Date);
        }

        public static TimeSpan operator -(iCalDateTime left, iCalDateTime right)
        {
            return left.UTC - right.UTC;
        }

        public static iCalDateTime operator -(iCalDateTime left, TimeSpan right)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(left.Value - right, left.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                left.TZID,
                left.Calendar);
        }

        public static iCalDateTime operator +(iCalDateTime left, TimeSpan right)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(left.Value + right, left.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                left.TZID,
                left.Calendar);
        }

        public static implicit operator iCalDateTime(DateTime left)
        {
            return new iCalDateTime(left);
        }

        #endregion

        #region Public Methods

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

        public iCalDateTime AddYears(int years)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddYears(years), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddMonths(int months)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddMonths(months), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddDays(int days)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddDays(days), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddHours(int hours)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddHours(hours), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddMinutes(int minutes)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddMinutes(minutes), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddSeconds(int seconds)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddSeconds(seconds), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        public iCalDateTime AddMilliseconds(int milliseconds)
        {
            return new iCalDateTime(
                DateTime.SpecifyKind(Value.AddMilliseconds(milliseconds), IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local),
                TZID,
                Calendar);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is iCalDateTime)
            {
                if (this.Equals(obj))
                    return 0;
                else if (this < (iCalDateTime)obj)
                    return -1;
                else if (this > (iCalDateTime)obj)
                    return 1;
            }
            throw new ArgumentException("obj must be a iCalDateTime");
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
