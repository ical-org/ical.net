using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DDay.iCal
{    
    /// <summary>
    /// A class that contains time zone information, and is usually accessed
    /// from an iCalendar object using the <see cref="DDay.iCal.iCalendar.GetTimeZone"/> method.        
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class iCalTimeZoneInfo : CalendarComponent, ITimeZoneInfo
    {
        TimeZoneInfoEvaluator m_Evaluator;

        public iCalTimeZoneInfo() : base()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            // base.Sequence = null;
            // iCalTimeZoneInfo does not allow sequence numbers
            // Perhaps we should have a custom serializer that fixes this?

            Initialize();
        }

        public iCalTimeZoneInfo(string name) : this()
        {
            Name = name;
        }

        void Initialize()
        {
            m_Evaluator = new TimeZoneInfoEvaluator(this);
            SetService(m_Evaluator);
        }

        private string _tzId;
        virtual public string TZID
        {
            get
            {
                if (_tzId == null)
                {
                    var tz = Parent as ITimeZone;
                    _tzId = tz?.TZID;
                }
                return _tzId;
            }
        }

        /// <summary>
        /// Returns the name of the current Time Zone.
        /// <example>
        ///     The following are examples:
        ///     <list type="bullet">
        ///         <item>EST</item>
        ///         <item>EDT</item>
        ///         <item>MST</item>
        ///         <item>MDT</item>
        ///     </list>
        /// </example>
        /// </summary>
        virtual public string TimeZoneName
        {
            get
            {
                return TimeZoneNames.FirstOrDefault();
            }
            set
            {
                TimeZoneNames.Clear();
                TimeZoneNames.Add(value);
            }
        }

        virtual public IUTCOffset TZOffsetFrom
        {
            get { return OffsetFrom; }
            set { OffsetFrom = value; }
        }

        virtual public IUTCOffset OffsetFrom
        {
            get { return Properties.Get<IUTCOffset>("TZOFFSETFROM"); }
            set { Properties.Set("TZOFFSETFROM", value); }
        }

        virtual public IUTCOffset OffsetTo
        {
            get { return Properties.Get<IUTCOffset>("TZOFFSETTO"); }
            set { Properties.Set("TZOFFSETTO", value); }
        }

        virtual public IUTCOffset TZOffsetTo
        {
            get { return OffsetTo; }
            set { OffsetTo = value; }
        }

        private IList<string> _tzNames = new List<string>();
        virtual public IList<string> TimeZoneNames
        {
            get { return _tzNames ?? (_tzNames = Properties.GetMany<string>("TZNAME")); }
            set { _tzNames = value; }
        }

        virtual public TimeZoneObservance? GetObservance(IDateTime dt)
        {
            if (Parent == null)
                throw new Exception("Cannot call GetObservance() on a TimeZoneInfo whose Parent property is null.");

            // Normalize date/time values within this time zone to a UTC value.
            var normalizedDt = dt.Value;
            if (string.Equals(dt.TZID, TZID))
            {
                dt = new iCalDateTime(OffsetTo.ToUTC(dt.Value));
                normalizedDt = OffsetTo.ToUTC(normalizedDt);
            }

            // Let's evaluate our time zone observances to find the 
            // observance that applies to this date/time value.
            var parentEval = Parent.GetService(typeof(IEvaluator)) as IEvaluator;
            if (parentEval != null)
            {
                // Evaluate the date/time in question.
                parentEval.Evaluate(Start, DateUtil.GetSimpleDateTimeData(Start), normalizedDt, true);
                foreach (var period in m_Evaluator.Periods)
                {
                    if (period.Contains(dt))
                        return new TimeZoneObservance(period, this);
                }
            }
            return null;
        }

        virtual public bool Contains(IDateTime dt)
        {
            var retval = GetObservance(dt);
            return (retval != null && retval.HasValue);
        }

        virtual public IDateTime DTStart
        {
            get { return Start; }
            set { Start = value; }
        }

        virtual public IDateTime Start
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        virtual public IList<IPeriodList> ExceptionDates
        {
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetMany<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        virtual public IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        virtual public IDateTime RecurrenceID
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        virtual public void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        virtual public HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(dt), true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(startTime), new iCalDateTime(endTime), true);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected bool Equals(iCalTimeZoneInfo other)
        {
            return base.Equals(other) && Equals(m_Evaluator, other.m_Evaluator);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((iCalTimeZoneInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (m_Evaluator != null ? m_Evaluator.GetHashCode() : 0);
                return hashCode;
            }
        }
    }    
}
