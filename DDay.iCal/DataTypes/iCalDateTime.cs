using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

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
        private iCalTimeZoneInfo _TimeZoneInfo = null;
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
                    iCalTimeZoneInfo tzi = TimeZoneInfo;
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
        /// Retrieves the <see cref="iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public iCalTimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (_TimeZoneInfo == null && TZID != null)
                {
                    if (Calendar != null)
                    {
                        ICalendarTimeZone tz = Calendar.GetTimeZone(TZID);
                        if (tz != null)
                            _TimeZoneInfo = tz.GetTimeZoneInfo(this);
                    }
                    else throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
                }
                return _TimeZoneInfo;
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
                if (TZID != null &&
                    TimeZoneInfo != null)
                {
                    return TimeZoneInfo.TimeZoneName;
                }
                return string.Empty;
            }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public DateTime Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public bool HasDate
        {
            get { return _HasDate; }
            set { _HasDate = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        public bool HasTime
        {
            get { return _HasTime; }
            set { _HasTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        public TZID TZID
        {
            get
            {
                if (_TZID == null && Parameters.ContainsKey("TZID"))
                    _TZID = new TZID(((CalendarParameter)Parameters["TZID"]).Values[0]);
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

        public iCalDateTime YearDate
        {
            get
            {
                return Copy().AddMonths(-Month + 1).AddDays(-Day + 1).AddHours(-Hour + 1).AddMinutes(-Minute + 1).AddSeconds(-Second + 1);
            }
        }

        public iCalDateTime MonthDate
        {
            get
            {
                return Copy().AddDays(-Day + 1).AddHours(-Hour + 1).AddMinutes(-Minute + 1).AddSeconds(-Second + 1);
            }
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
            Initialize(value, tzid, iCal);
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second) : this()
        {
            Initialize(year, month, day, hour, minute, second, null, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, TZID tzid, iCalendar iCal) : this()            
        {
            Initialize(year, month, day, hour, minute, second, tzid, iCal);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, TZID tzid, iCalendar iCal)
            : this(year, month, day, 0, 0, 0, tzid, iCal) { }

        // FIXME: what do we do with this constructor?
        // I don't really like it...

        //public iCalDateTime(CalendarProperty p) : this(p.Value)
        //{
        //    this.iCalendar = p.Calendar;
        //    if (p.Parameters.ContainsKey("VALUE"))
        //        this.Parameters["VALUE"] = p.Parameters["VALUE"];
        //    if (p.Parameters.ContainsKey("TZID"))
        //        this.TZID = p.Parameters["TZID"].Values[0];                
        //}

        private void Initialize(int year, int month, int day, int hour, int minute, int second, TZID tzid, iCalendar iCal)
        {
            Initialize(CoerceDateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid, iCal);            
        }

        private void Initialize(DateTime value, TZID tzid, iCalendar iCal)
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            this.TZID = tzid;
            this.iCalendar = iCal;
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

        public override void CopyFrom(ICopyable obj)
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

        public override bool TryParse(string value, ref ICalendarObject obj)
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

                DateTime setDateTime = CoerceDateTime(year, month, date, hour, minute, second, DateTimeKind.Utc);
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

        public DateTime ToTimeZone(iCalTimeZoneInfo tzi)
        {
            DateTime value = UTC;

            int mult = tzi.TZOffsetTo.Positive ? 1 : -1;
            value = value.AddHours(tzi.TZOffsetTo.Hours * mult);
            value = value.AddMinutes(tzi.TZOffsetTo.Minutes * mult);
            value = value.AddSeconds(tzi.TZOffsetTo.Seconds * mult);
            value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            return value;
        }
                
        public DateTime ToTimeZone(string tzid)
        {
            if (!string.IsNullOrEmpty(tzid))
            {
                if (iCalendar != null)
                {
                    iCalTimeZone tz = iCalendar.GetTimeZone(tzid);
                    if (tz != null)
                    {
                        iCalTimeZoneInfo tzi = tz.GetTimeZoneInfo(this);
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
