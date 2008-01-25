using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
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
    public class iCalDateTime : 
        iCalDataType,
        IComparable,
        IFormattable
    {
        #region Private Fields

        private DateTime _Value;
        private bool _HasDate = false;
        private bool _HasTime = false;
        private TZID _TZID = null;
        private TimeZoneInfo _TimeZoneInfo = null;
        private bool _IsUniversalTime = false;

        #endregion

        #region Public Properties

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
                    TimeZoneInfo tzi = TimeZoneInfo;
                    if (tzi != null)
                    {
                        int mult = tzi.TZOffsetTo.Positive ? -1 : 1;
                        value = value.AddHours(tzi.TZOffsetTo.Hours * mult);
                        value = value.AddMinutes(tzi.TZOffsetTo.Minutes * mult);
                        value = value.AddSeconds(tzi.TZOffsetTo.Seconds * mult);
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
        /// Retrieves the <see cref="TimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (_TimeZoneInfo == null && TZID != null)
                {
                    if (iCalendar != null)
                    {
                        iCalTimeZone tz = iCalendar.GetTimeZone(TZID);
                        if (tz != null)
                            _TimeZoneInfo = tz.GetTimeZoneInfo(this);
                    }
                    else throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
                }
                return _TimeZoneInfo;
            }
        }

        public bool IsUniversalTime
        {
            get { return _IsUniversalTime; }
            set { _IsUniversalTime = value; }
        }

        /// <summary>
        /// Sets/Gets the iCalendar associated with this <see cref="iCalDateTime"/>.
        /// <note>
        /// This property cannot be null when the <see cref="TZID"/>
        /// has been set for this <see cref="iCalDateTime"/> object.
        /// </note>
        /// </summary>
        public new iCalendar iCalendar
        {
            get { return base.iCalendar; }
            set { Parent = value; }
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

        public DateTime Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public bool HasDate
        {
            get { return _HasDate; }
            set { _HasDate = value; }
        }        

        public bool HasTime
        {
            get { return _HasTime; }
            set { _HasTime = value; }
        }

        public TZID TZID
        {
            get
            {
                if (_TZID == null && Parameters.ContainsKey("TZID"))
                    _TZID = new TZID(((Parameter)Parameters["TZID"]).Values[0]);
                return _TZID;
            }
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

        #endregion

        #region Constructors

        public iCalDateTime() : base() { }
        public iCalDateTime(iCalDateTime value) : this()
        {
            CopyFrom(value);
        }
        public iCalDateTime(string value) : this()
        {
            CopyFrom(Parse(value));
        }
        public iCalDateTime(DateTime value) : this(value, null, null) {}
        public iCalDateTime(DateTime value, TZID tzid, iCalendar iCal) : this()
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            this.TZID = tzid;
            this.iCalendar = iCal;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local))
        {
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, TZID tzid, iCalendar iCal)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid, iCal)
        {
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, TZID tzid, iCalendar iCal)
            : this(year, month, day, 0, 0, 0, tzid, iCal) { }
        
        #endregion

        #region Overrides
               
        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is iCalDateTime)
            {
                iCalDateTime dt = (iCalDateTime)obj;
                this.Value = dt.Value;
                this.HasDate = dt.HasDate;
                this.HasTime = dt.HasTime;
                this.TZID = dt.TZID;
                this.iCalendar = dt.iCalendar;
                this.IsUniversalTime = dt.IsUniversalTime;
            }
            base.CopyFrom(obj);
        }

        virtual public void MergeWith(iCalDateTime dt)
        {
            if (iCalendar == null)
                iCalendar = dt.iCalendar;
            if (TZID == null)
                TZID = dt.TZID;
            IsUniversalTime = dt.IsUniversalTime;
        }

        public override bool TryParse(string value, ref object obj)
        {
            string[] values = value.Split('T');

            if (obj == null)
                obj = new iCalDateTime();
            iCalDateTime dt = (iCalDateTime)obj;

            Match match = Regex.Match(value, @"^((\d{4})(\d{2})(\d{2}))?T?((\d{2})(\d{2})(\d{2})(Z)?)?$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;
            else
            {
                DateTime now = DateTime.Now;

                int year = now.Year;
                int month = now.Month;
                int date = now.Day;
                int hour = 0;
                int minute = 0;
                int second = 0;

                if (match.Groups[1].Success)
                {
                    dt.HasDate = true;
                    year = Convert.ToInt32(match.Groups[2].Value);
                    month = Convert.ToInt32(match.Groups[3].Value);
                    date = Convert.ToInt32(match.Groups[4].Value);
                }
                if (match.Groups[5].Success)
                {
                    dt.HasTime = true;
                    hour = Convert.ToInt32(match.Groups[6].Value);
                    minute = Convert.ToInt32(match.Groups[7].Value);
                    second = Convert.ToInt32(match.Groups[8].Value);
                }
                
                if (match.Groups[9].Success)
                    dt.IsUniversalTime = true;
                DateTime setDateTime = new DateTime(year, month, date, hour, minute, second, DateTimeKind.Utc);                               

                dt.Value = setDateTime;
            }
            return true;
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

        public static TimeSpan operator -(iCalDateTime left, iCalDateTime right)
        {
            return left.UTC - right.UTC;
        }

        public static iCalDateTime operator -(iCalDateTime left, TimeSpan right)
        {
            iCalDateTime copy = left.Copy();
            copy.Value -= right;
            return copy;
        }

        public static iCalDateTime operator +(iCalDateTime left, TimeSpan right)
        {
            iCalDateTime copy = left.Copy();
            copy.Value += right;
            return copy;
        }

        public static implicit operator iCalDateTime(DateTime left)
        {
            return new iCalDateTime(left);
        }

        #endregion

        #region Public Methods

        public new iCalDateTime Copy()
        {
            return (iCalDateTime)base.Copy();
        }

        public DateTime ToTimeZone(TimeZoneInfo tzi)
        {
            DateTime value = UTC;

            int mult = tzi.TZOffsetTo.Positive ? 1 : -1;
            value = value.AddHours(tzi.TZOffsetTo.Hours * mult);
            value = value.AddMinutes(tzi.TZOffsetTo.Minutes * mult);
            value = value.AddSeconds(tzi.TZOffsetTo.Seconds * mult);
            value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            return value;
        }

        // FIXME: what's wrong with this method?? Start here
        public DateTime ToTimeZone(string tzid)
        {
            if (!string.IsNullOrEmpty(tzid))
            {
                if (iCalendar != null)
                {
                    iCalTimeZone tz = iCalendar.GetTimeZone(tzid);
                    if (tz != null)
                    {
                        TimeZoneInfo tzi = tz.GetTimeZoneInfo(this);
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
            iCalDateTime dt = Copy();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        public iCalDateTime AddMonths(int months)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddMonths(months);
            return dt;
        }

        public iCalDateTime AddDays(int days)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddDays(days);
            return dt;
        }

        public iCalDateTime AddHours(int hours)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public iCalDateTime AddMinutes(int minutes)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public iCalDateTime AddSeconds(int seconds)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public iCalDateTime AddMilliseconds(int milliseconds)
        {
            iCalDateTime dt = Copy();
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
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
