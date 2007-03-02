using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal.Components;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="Date_Time"/>
    /// class handles time zone differences, and integrates seamlessly into
    /// the iCalendar framework.
    /// </remarks>
    /// </summary>
    [DebuggerDisplay("{HasTime ? Value.ToString() : Value.ToShortDateString()}")]
    public class Date_Time : iCalDataType, IComparable
    {
        #region Private Fields

        private DateTime m_Value;
        private bool m_HasDate = false;
        private bool m_HasTime = false;
        private TZID m_TZID = null;
        private DDay.iCal.Components.TimeZone.TimeZoneInfo m_TimeZoneInfo = null;        

        #endregion

        #region Public Properties

        /// <summary>
        /// Converts the date/time to the computer's local date/time.
        /// </summary>
        public DateTime Local
        {
            get
            {
                if (!HasTime)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Local);
                else if (Value.Kind == DateTimeKind.Local &&
                    TimeZoneInfo != null)
                    return UTC.ToLocalTime();
                else return Value.ToLocalTime();
            }
        }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        public DateTime UTC
        {
            get
            {
                if (!HasTime)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Utc);
                else if (Value.Kind == DateTimeKind.Local)
                {
                    DateTime value = Value;
                    if (TimeZoneInfo != null)
                    {
                        int mult = TimeZoneInfo.TZOffsetTo.Positive ? -1 : 1;
                        value = value.AddHours(TimeZoneInfo.TZOffsetTo.Hours * mult);
                        value = value.AddMinutes(TimeZoneInfo.TZOffsetTo.Minutes * mult);
                        value = value.AddSeconds(TimeZoneInfo.TZOffsetTo.Seconds * mult);
                        value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                    }
                    else value = value.ToUniversalTime();
                    return value;
                }
                else return Value.ToUniversalTime();
            }
        }

        /// <summary>
        /// Retrieves the <see cref="TimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public DDay.iCal.Components.TimeZone.TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (m_TimeZoneInfo == null && TZID != null)
                {
                    if (iCalendar != null)
                    {
                        DDay.iCal.Components.TimeZone tz = iCalendar.GetTimeZone(TZID);
                        if (tz != null)
                            m_TimeZoneInfo = tz.GetTimeZoneInfo(this);
                    }
                    else throw new ApplicationException("The Date_Time object must have an iCalendar associated with it in order to use TimeZones.");
                }
                return m_TimeZoneInfo;
            }
        }

        /// <summary>
        /// Sets/Gets the iCalendar associated with this <see cref="Date_Time"/>.
        /// <note>
        /// This property cannot be <see cref="null"/> when the <see cref="TZID"/>
        /// has been set for this <see cref="Date_Time"/> object.
        /// </note>
        /// </summary>
        public iCalendar iCalendar
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
            get { return m_Value; }
            set { m_Value = value; }
        }

        public bool HasDate
        {
            get { return m_HasDate; }
            set { m_HasDate = value; }
        }        

        public bool HasTime
        {
            get { return m_HasTime; }
            set { m_HasTime = value; }
        }

        public TZID TZID
        {
            get
            {
                if (m_TZID == null && Parameters.ContainsKey("TZID"))
                    m_TZID = new TZID(((Parameter)Parameters["TZID"]).Values[0]);
                return m_TZID;
            }
            set { m_TZID = value; }
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

        public DateTimeKind Kind
        {
            get { return Value.Kind; }
            set
            {
                // Change the type of Date/Time value
                Value = DateTime.SpecifyKind(Value, value);
            }
        }

        #endregion

        #region Constructors

        public Date_Time() : base() { }
        public Date_Time(Date_Time value) : this()
        {
            CopyFrom(value);
        }
        public Date_Time(string value) : this()
        {
            CopyFrom(Parse(value));
        }
        public Date_Time(DateTime value) : this(value, null, null) {}
        public Date_Time(DateTime value, TZID tzid, iCalendar iCal) : this()
        {
            this.Value = value;
            this.HasDate = true;
            this.HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            this.TZID = tzid;
            this.iCalendar = iCal;
        }
        public Date_Time(int year, int month, int day, int hour, int minute, int second)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local))
        {
            HasTime = true;
        }
        public Date_Time(int year, int month, int day, int hour, int minute, int second, TZID tzid, iCalendar iCal)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid, iCal)
        {
            HasTime = true;
        }
        public Date_Time(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public Date_Time(int year, int month, int day, TZID tzid, iCalendar iCal)
            : this(year, month, day, 0, 0, 0, tzid, iCal) { }
        
        #endregion

        #region Overrides
               
        public override void CopyFrom(object obj)
        {
            if (obj is Date_Time)
            {
                Date_Time dt = (Date_Time)obj;
                this.Value = dt.Value;
                this.HasDate = dt.HasDate;
                this.HasTime = dt.HasTime;
                this.TZID = dt.TZID;
                this.iCalendar = dt.iCalendar;                
            }
            base.CopyFrom(obj);
        }

        virtual public void MergeWith(Date_Time dt)
        {
            if (iCalendar == null)
                iCalendar = dt.iCalendar;
            if (TZID == null)
                TZID = dt.TZID;
        }

        public override bool TryParse(string value, ref object obj)
        {
            string[] values = value.Split('T');

            if (obj == null)
                obj = new Date_Time();
            Date_Time dt = (Date_Time)obj;

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
                
                DateTimeKind dtk = match.Groups[9].Success ? DateTimeKind.Utc : DateTimeKind.Local;
                if (dtk == DateTimeKind.Utc && TZID != null)
                    throw new ApplicationException("TZID cannot be speicified on a UTC time");
                DateTime setDateTime = new DateTime(year, month, date, hour, minute, second, dtk);                               

                dt.Value = setDateTime;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is Date_Time)
                return ((Date_Time)obj).UTC.Equals(UTC);
            else if (obj is DateTime)
                return ((DateTime)obj).ToUniversalTime() == UTC;
            return false;            
        }

        public override string ToString()
        {
            if (HasDate)
                return Value.ToString();
            else return Value.ToShortDateString();
        }

        #endregion

        #region Operators

        public static bool operator <(Date_Time left, Date_Time right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC < right.UTC;
            else return left.UTC.Date < right.UTC.Date;
        }

        public static bool operator >(Date_Time left, Date_Time right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC > right.UTC;
            else return left.UTC.Date > right.UTC.Date;
        }

        public static bool operator <=(Date_Time left, Date_Time right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC <= right.UTC;
            else return left.UTC.Date <= right.UTC.Date;
        }

        public static bool operator >=(Date_Time left, Date_Time right)
        {
            if (left.HasTime || right.HasTime)
                return left.UTC >= right.UTC;
            else return left.UTC.Date >= right.UTC.Date;
        }

        public static TimeSpan operator -(Date_Time left, Date_Time right)
        {
            return left.UTC - right.UTC;
        }

        public static Date_Time operator -(Date_Time left, TimeSpan right)
        {
            Date_Time copy = left.Copy();
            copy.Value -= right;
            return copy;
        }

        public static Date_Time operator +(Date_Time left, TimeSpan right)
        {
            Date_Time copy = left.Copy();
            copy.Value += right;
            return copy;
        }

        public static implicit operator Date_Time(DateTime left)
        {
            return new Date_Time(left.ToUniversalTime());
        }

        #endregion

        #region Public Methods

        public Date_Time Copy()
        {
            return (Date_Time)base.Copy();
        }

        public void SetKind(DateTimeKind kind)
        {
            switch(kind)
            {
                case DateTimeKind.Local:
                    Value = Local;
                    break;
                case DateTimeKind.Utc:
                    Value = UTC;
                    break;
                default: break;
            }            
        }

        public Date_Time AddYears(int years)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        public Date_Time AddMonths(int months)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddMonths(months);
            return dt;
        }

        public Date_Time AddDays(int days)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddDays(days);
            return dt;
        }

        public Date_Time AddHours(int hours)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public Date_Time AddMinutes(int minutes)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public Date_Time AddSeconds(int seconds)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public Date_Time AddMilliseconds(int milliseconds)
        {
            Date_Time dt = Copy();
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Date_Time)
            {
                if (this.Equals(obj))
                    return 0;
                else if (this < (Date_Time)obj)
                    return -1;
                else if (this > (Date_Time)obj)
                    return 1;
            }
            throw new ArgumentException("obj must be a Date_Time");
        }

        #endregion
    }
}
