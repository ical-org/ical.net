using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using DDay.iCal.Serialization.iCalendar;
using System.IO;

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
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class iCalDateTime :
        EncodableDataType,
        IDateTime
    {
        #region Static Public Properties

        static public iCalDateTime Now
        {
            get
            {
                return new iCalDateTime(DateTime.Now);
            }
        }

        static public iCalDateTime Today
        {
            get
            {
                return new iCalDateTime(DateTime.Today);
            }            
        }

        #endregion

        #region Private Fields

        private DateTime _Value;
        private bool _HasDate;
        private bool _HasTime;
        private TimeZoneObservance? _TimeZoneObservance;
        private bool _IsUniversalTime;

        #endregion

        #region Constructors

        public iCalDateTime() { }
        public iCalDateTime(IDateTime value)
        {
            Initialize(value.Value, value.TZID, null);
        }
        public iCalDateTime(DateTime value) : this(value, null) {}
        public iCalDateTime(DateTime value, string tzid) 
        {
            Initialize(value, tzid, null);
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(year, month, day, hour, minute, second, null, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid)
        {
            Initialize(year, month, day, hour, minute, second, tzid, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal)
        {
            Initialize(year, month, day, hour, minute, second, tzid, iCal);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, string tzid)
            : this(year, month, day, 0, 0, 0, tzid) { }

        public iCalDateTime(string value)
        {
            DateTimeSerializer serializer = new DateTimeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

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
            this.AssociatedObject = iCal;
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

        protected TimeZoneObservance? GetTimeZoneObservance()
        {
            if (_TimeZoneObservance == null && 
                TZID != null && 
                Calendar != null)
            {
                ITimeZone tz = Calendar.GetTimeZone(TZID);
                if (tz != null)
                    _TimeZoneObservance = tz.GetTimeZoneObservance(this);
            }
            return _TimeZoneObservance;
        }

        #endregion

        #region Overrides

        public override ICalendarObject AssociatedObject
        {
            get
            {
                return base.AssociatedObject;
            }
            set
            {
                if (!object.Equals(AssociatedObject, value))
                {
                    base.AssociatedObject = value;
                    _TimeZoneObservance = null;
                }
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            IDateTime dt = obj as IDateTime;
            if (dt != null)
            {
                _Value = dt.Value;
                _IsUniversalTime = dt.IsUniversalTime;                
                _HasDate = dt.HasDate;
                _HasTime = dt.HasTime;
                
                AssociateWith(dt);
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj is IDateTime)
            {
                this.AssociateWith((IDateTime)obj);
                return ((IDateTime)obj).UTC.Equals(UTC);
            }
            else if (obj is DateTime)
            {
                iCalDateTime dt = (iCalDateTime)obj;
                this.AssociateWith(dt);
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
            string tz = TimeZoneName;
            if (!string.IsNullOrEmpty(tz))
                tz = " " + tz;

            if (HasTime && HasDate)
                return Value.ToString() + tz;
            else if (HasTime)
                return Value.TimeOfDay.ToString() + tz;
            else
                return Value.ToShortDateString() + tz;
        }

        #endregion

        #region Operators

        public static bool operator <(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC < right.UTC;
            else return left.UTC.Date < right.UTC.Date;
        }

        public static bool operator >(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC > right.UTC;
            else return left.UTC.Date > right.UTC.Date;
        }

        public static bool operator <=(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC <= right.UTC;
            else return left.UTC.Date <= right.UTC.Date;
        }

        public static bool operator >=(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC >= right.UTC;
            else return left.UTC.Date >= right.UTC.Date;
        }

        public static bool operator ==(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC.Equals(right.UTC);
            else return left.UTC.Date.Equals(right.UTC.Date);
        }

        public static bool operator !=(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return !left.UTC.Equals(right.UTC);
            else return !left.UTC.Date.Equals(right.UTC.Date);
        }

        public static TimeSpan operator -(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);
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
                else if (TZID != null)
                {
                    DateTime value = Value;

                    // Get the Time Zone Observance, if possible
                    TimeZoneObservance? tzi = TimeZoneObservance;
                    if (tzi == null || !tzi.HasValue)
                        tzi = GetTimeZoneObservance();

                    if (tzi != null && tzi.HasValue)
                    {
                        Debug.Assert(tzi.Value.TimeZoneInfo.OffsetTo != null);
                        return DateTime.SpecifyKind(tzi.Value.TimeZoneInfo.OffsetTo.ToUTC(value), DateTimeKind.Utc);
                    }                    
                }
                 
                // Fallback to the OS-conversion
                return DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();
            }
        }

        /// <summary>
        /// Gets the <see cref="iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public TimeZoneObservance? TimeZoneObservance
        {
            get
            {
                return _TimeZoneObservance;
            }
            set
            {
                _TimeZoneObservance = value;
            }
        }

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
                else if (_TimeZoneObservance != null && _TimeZoneObservance.HasValue)
                    return _TimeZoneObservance.Value.TimeZoneInfo.TimeZoneName;
                return string.Empty;
            }
        }

        public DateTime Value
        {
            get { return _Value; }
            set
            {
                if (!object.Equals(_Value, value))
                {
                    _Value = value;

                    // Reset the time zone info if the new date/time doesn't
                    // fall within this time zone observance.
                    if (_TimeZoneObservance != null && 
                        _TimeZoneObservance.HasValue &&
                        !_TimeZoneObservance.Value.Period.Contains(this))
                        _TimeZoneObservance = null;
                }
                    
            }
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

        public string TZID
        {
            get { return Parameters.Get("TZID"); }
            set
            {
                if (!object.Equals(TZID, value))
                {
                    Parameters.Set("TZID", value);
                    _TimeZoneObservance = null;
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

        public IDateTime ToTimeZone(ITimeZoneInfo tzi)
        {
            return new iCalDateTime(tzi.OffsetTo.ToLocal(UTC));
        }

        public IDateTime ToTimeZone(string tzid)
        {
            if (tzid != null)
            {
                if (Calendar != null)
                {
                    ITimeZone tz = Calendar.GetTimeZone(tzid);
                    if (tz != null)
                    {
                        TimeZoneObservance? tzi = tz.GetTimeZoneObservance(this);
                        if (tzi != null && tzi.HasValue)
                            return ToTimeZone(tzi.Value.TimeZoneInfo);
                    }
                    // FIXME: sometimes a calendar is perfectly valid but the time zone
                    // could not be resolved.  What should we do here?
                    //throw new Exception("The '" + tzid + "' time zone could not be resolved.");

                    return Copy<IDateTime>();
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
            if (!dt.HasTime && hours % 24 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public IDateTime AddMinutes(int minutes)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && minutes % 1440 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public IDateTime AddSeconds(int seconds)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && seconds % 86400 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public IDateTime AddMilliseconds(int milliseconds)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && milliseconds % 86400000 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
        }

        public IDateTime AddTicks(long ticks)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddTicks(ticks);
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

        public void AssociateWith(IDateTime dt)
        {
            if (AssociatedObject == null && dt.AssociatedObject != null)
                AssociatedObject = dt.AssociatedObject;
            else if (AssociatedObject != null && dt.AssociatedObject == null)
                dt.AssociatedObject = AssociatedObject;

            // If these share the same TZID, then let's see if we
            // can share the time zone observance also!
            if (TZID != null && string.Equals(TZID, dt.TZID))
            {
                if (TimeZoneObservance != null && dt.TimeZoneObservance == null)
                {
                    IDateTime normalizedDt = new iCalDateTime(TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(dt.Value));
                    if (TimeZoneObservance.Value.Contains(normalizedDt))
                        dt.TimeZoneObservance = TimeZoneObservance;
                }
                else if (dt.TimeZoneObservance != null && TimeZoneObservance == null)
                {
                    IDateTime normalizedDt = new iCalDateTime(dt.TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(Value));
                    if (dt.TimeZoneObservance.Value.Contains(normalizedDt))
                        TimeZoneObservance = dt.TimeZoneObservance;
                }
            }
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
