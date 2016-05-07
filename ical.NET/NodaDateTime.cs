//using System;
//using NodaTime;
//using NodaTime.TimeZones;
//using NodaTime.Utility;

//namespace DDay.iCal
//{
//    public class NodaDateTime //: INodaDateTime //IDateTime
//    {
//        public NodaDateTime(ZonedDateTime zonedDateTime)
//        {
//            Value = zonedDateTime;
//        }

//        /// <summary>
//        /// Converts the date/time to this computer's local date/time.
//        /// </summary>
//        public ZonedDateTime AsSystemLocal
//        {
//            get
//            {
//                var localTimeZone = BclDateTimeZone.ForSystemDefault();
//                var systemLocal = new ZonedDateTime(Value.LocalDateTime, localTimeZone, Value.Offset);
//                return systemLocal;
//            }
//        }

//        /// <summary>
//        /// Converts the date/time to UTC (Coordinated Universal Time)
//        /// </summary>
//        public ZonedDateTime AsUtc => Value.WithZone(DateTimeZone.Utc);

//        /// <summary>
//        /// Retrieves the <see cref="iCalTimeZoneInfo"/> object for the time
//        /// zone set by <see cref="TzId"/>.
//        /// </summary>
//        //TimeZoneObservance? TimeZoneObservance { get; set; }

//        /// <summary>
//        /// Gets/sets whether the Value of this date/time represents
//        /// a universal time.
//        /// </summary>
//        public bool IsUniversalTime => Value.Zone.Equals(DateTimeZone.Utc);

//        /// <summary>
//        /// Gets the time zone name this time is in, if it references a time zone.
//        /// </summary>
//        public string TimeZoneName => TzId;

//        /// <summary>
//        /// Gets/sets the underlying DateTime value stored.  This should always
//        /// use DateTimeKind.Utc, regardless of its actual representation.
//        /// Use IsUniversalTime along with the TZID to control how this
//        /// date/time is handled.
//        /// </summary>
//        public ZonedDateTime Value { get; set; }

//        /// <summary>
//        /// Gets whether or not this date/time value contains a 'date' part.
//        /// </summary>
//        public bool HasDate => true;

//        /// <summary>
//        /// Gets/sets whether or not this date/time value contains a 'time' part.
//        /// </summary>
//        public bool HasTime => Value.TickOfDay > 0;

//        /// <summary>
//        /// Gets the time zone ID for this date/time value.
//        /// </summary>
//        public string TzId => Value.Zone.Id;

//        /// <summary>
//        /// Gets the year for this date/time value.
//        /// </summary>
//        public int Year => Value.Year;

//        /// <summary>
//        /// Gets the month for this date/time value.
//        /// </summary>
//        public int Month => Value.Month;

//        /// <summary>
//        /// Gets the day for this date/time value.
//        /// </summary>
//        public int Day => Value.Day;

//        /// <summary>
//        /// Gets the hour for this date/time value.
//        /// </summary>
//        public int Hour => Value.Hour;

//        /// <summary>
//        /// Gets the minute for this date/time value.
//        /// </summary>
//        public int Minute => Value.Minute;

//        /// <summary>
//        /// Gets the second for this date/time value.
//        /// </summary>
//        public int Second => Value.Second;

//        /// <summary>
//        /// Gets the millisecond for this date/time value.
//        /// </summary>
//        public int Millisecond => Value.Millisecond;

//        /// <summary>
//        /// Gets the ticks for this date/time value.
//        /// </summary>
//        public long Ticks => Value.ToDateTimeUtc().Ticks;

//        /// <summary>
//        /// Gets the DayOfWeek for this date/time value.
//        /// </summary>
//        public DayOfWeek DayOfWeek => BclConversions.ToDayOfWeek(Value.IsoDayOfWeek);

//        /// <summary>
//        /// Gets the date portion of the date/time value.
//        /// </summary>
//        public LocalDate Date => Value.Date;

//        ///// <summary>
//        ///// Converts the date/time value to a local time
//        ///// within the specified time zone.
//        ///// </summary>
//        //IDateTime ToTimeZone(TimeZoneObservance tzo);

//        ///// <summary>
//        ///// Converts the date/time value to a local time
//        ///// within the specified time zone.
//        ///// </summary>
//        public INodaDateTime ToTimeZone(string tzId) => new NodaDateTime(Value.WithZone(DateUtil.GetZone(tzId)));
//        //IDateTime ToTimeZone(ITimeZone tz);

//        public INodaDateTime Add(Duration duration) => new NodaDateTime(Value.Plus(duration));
//        public INodaDateTime Subtract(Duration duration) => new NodaDateTime(Value.Minus(duration));
//        public Duration Subtract(INodaDateTime dt) => Value.ToInstant() - dt.Value.ToInstant();

//        public INodaDateTime AddYears(int years) => new NodaDateTime(DateUtil.AddYears(Value, years));
//        public INodaDateTime AddMonths(int months) => new NodaDateTime(DateUtil.AddMonths(Value, months));
//        public INodaDateTime AddDays(int days) => new NodaDateTime(Value.Plus(Duration.FromStandardDays(days)));
//        public INodaDateTime AddHours(int hours) => new NodaDateTime(Value.Plus(Duration.FromHours(hours)));
//        public INodaDateTime AddMinutes(int minutes) => new NodaDateTime(Value.Plus(Duration.FromMinutes(minutes)));
//        public INodaDateTime AddSeconds(int seconds) => new NodaDateTime(Value.Plus(Duration.FromSeconds(seconds)));
//        public INodaDateTime AddMilliseconds(int milliseconds) => new NodaDateTime(Value.Plus(Duration.FromMilliseconds(milliseconds)));
//        public INodaDateTime AddTicks(long ticks) => new NodaDateTime(Value.Plus(Duration.FromTicks(ticks)));

//        public bool LessThan(INodaDateTime dt) => Value < dt.Value;
//        public bool GreaterThan(INodaDateTime dt) => Value > dt.Value;
//        public bool LessThanOrEqual(INodaDateTime dt) => Value <= dt.Value;
//        public bool GreaterThanOrEqual(INodaDateTime dt) => Value >= dt.Value;

//        //void AssociateWith(INodaDateTime dt);

//        public override void CopyFrom(ICopyable obj)
//        {
//            base.CopyFrom(obj);

//            var dt = obj as INodaDateTime;
//            if (dt != null)
//            {
//                Value = dt.Value;
//                AssociateWith(dt);
//            }
//        }

//        public override ICalendarObject AssociatedObject
//        {
//            get
//            {
//                return base.AssociatedObject;
//            }
//            set
//            {
//                if (!Equals(AssociatedObject, value))
//                {
//                    base.AssociatedObject = value;
//                }
//            }
//        }

//        protected bool Equals(NodaDateTime other)
//        {
//            return Value.Equals(other.Value);
//        }

//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj))
//            {
//                return false;
//            }
//            if (ReferenceEquals(this, obj))
//            {
//                return true;
//            }
//            if (obj.GetType() != GetType())
//            {
//                return false;
//            }
//            return Equals((NodaDateTime) obj);
//        }

//        public override int GetHashCode()
//        {
//            return Value.GetHashCode();
//        }

//        public int CompareTo(INodaDateTime dt)
//        {
//            if (Value == dt.Value)
//            {
//                return 0;
//            }
//            if (this.Value < dt.Value)
//            {
//                return -1;
//            }
//            if (this.Value > dt.Value)
//            {
//                return 1;
//            }
//            throw new Exception("An error occurred while comparing two INodaDateTime values.");
//        }

//        public override string ToString()
//        {
//            return ToString(null, null);
//        }

//        public string ToString(string format)
//        {
//            return ToString(format, null);
//        }

//        public string ToString(string format, IFormatProvider formatProvider)
//        {
//            var tz = TimeZoneName;
//            if (!string.IsNullOrEmpty(tz))
//            {
//                tz = " " + tz;
//            }

//            if (format != null)
//            {
//                return Value.ToString(format, formatProvider) + tz;
//            }
//            if (HasTime && HasDate)
//            {
//                return Value + tz;
//            }
//            if (HasTime)
//            {
//                return Value.TimeOfDay + tz;
//            }
//            return Value + tz;
//        }

//        public void AssociateWith(INodaDateTime dt)
//        {
//            if (AssociatedObject == null && dt.AssociatedObject != null)
//            {
//                AssociatedObject = dt.AssociatedObject;
//            }
//            else if (AssociatedObject != null && dt.AssociatedObject == null)
//            {
//                dt.AssociatedObject = AssociatedObject;
//            }
//        }
//    }
//}

