using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization;

namespace DDay.iCal
{    
    /// <summary>
    /// A class that contains time zone information, and is usually accessed
    /// from an iCalendar object using the <see cref="DDay.iCal.iCalendar.GetTimeZone"/> method.        
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalTimeZoneInfo", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class iCalTimeZoneInfo : 
        CalendarComponent,
        ITimeZoneInfo
    {
        #region Private Fields

        TimeZoneInfoEvaluator m_Evaluator;
        DateTime m_End;

        #endregion

        #region Constructors

        public iCalTimeZoneInfo() : base()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            //base.Sequence = null; // iCalTimeZoneInfo does not allow sequence numbers

            Initialize();
        }
        public iCalTimeZoneInfo(string name) : this()
        {
            this.Name = name;
        }

        void Initialize()
        {
            m_Evaluator = new TimeZoneInfoEvaluator(this);

            RecurrenceDates = new List<IPeriodList>();
            RecurrenceRules = new List<IRecurrencePattern>();
            ExceptionDates = new List<IPeriodList>();
            ExceptionRules = new List<IRecurrencePattern>();
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override object GetService(Type serviceType)
        {
            if (typeof(IEvaluator).IsAssignableFrom(serviceType))
                return m_Evaluator;
            return null;
        }

        public override bool Equals(object obj)
        {
            iCalTimeZoneInfo tzi = obj as iCalTimeZoneInfo;
            if (tzi != null)
            {
                return object.Equals(TimeZoneName, tzi.TimeZoneName) &&
                    object.Equals(OffsetFrom, tzi.OffsetFrom) &&
                    object.Equals(OffsetTo, tzi.OffsetTo);
            }
            return base.Equals(obj);
        }
                              
        #endregion

        #region ITimeZoneInfo Members

        virtual public string TZID
        {
            get
            {
                ITimeZone tz = Parent as ITimeZone;
                if (tz != null)
                    return tz.TZID;
                return null;
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
                IList<string> tzNames = TimeZoneNames;
                if (tzNames != null &&
                    tzNames.Count > 0)
                    return tzNames[0];
                return null;
            }
            set
            {
                IList<string> tzNames = TimeZoneNames;
                if (tzNames != null &&
                    tzNames.Count > 0)
                    tzNames[0] = value;
                else
                {
                    if (value != null)
                        tzNames = new List<string>(new string[] { value });
                    else
                        tzNames = null;
                }
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

        virtual public IList<string> TimeZoneNames
        {
            get { return Properties.GetList<string>("TZNAME"); }
            set { Properties.SetList<string>("TZNAME", value); }
        }

        virtual public TimeZoneObservance? GetObservance(IDateTime dt)
        {
            if (Parent == null)
                throw new Exception("Cannot call GetObservance() on a TimeZoneInfo whose Parent property is null.");

            // Normalize date/time values within this time zone to a UTC value.
            DateTime normalizedDt = dt.Value;
            if (string.Equals(dt.TZID, TZID))
            {
                dt = new iCalDateTime(OffsetTo.Offset(dt.Value));
                normalizedDt = OffsetTo.Offset(normalizedDt);
            }
                        
            // Let's evaluate our time zone observances to find the 
            // observance that applies to this date/time value.
            IEvaluator parentEval = Parent.GetService(typeof(IEvaluator)) as IEvaluator;
            if (parentEval != null)
            {
                // Evaluate the date/time in question.
                parentEval.Evaluate(Start, DateUtil.GetSimpleDateTimeData(Start), normalizedDt);
                foreach (IPeriod period in m_Evaluator.Periods)
                {   
                    if (period.Contains(dt))
                        return new TimeZoneObservance(period, this);
                }
            }
            return null;
        }

        virtual public bool Contains(IDateTime dt)
        {
            TimeZoneObservance? retval = GetObservance(dt);
            return (retval != null && retval.HasValue);
        }

        #endregion

        #region IRecurrable Members

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
            get { return Properties.GetList<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetList<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetList<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        virtual public IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetList<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        virtual public IDateTime RecurrenceID
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        #endregion

        #region IRecurrable Members

        virtual public void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        virtual public IList<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt);
        }

        virtual public IList<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(dt));
        }

        virtual public IList<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime);
        }

        virtual public IList<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(startTime), new iCalDateTime(endTime));
        }

        #endregion
    }    
}
