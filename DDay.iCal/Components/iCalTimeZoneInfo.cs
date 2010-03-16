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

        RecurringObjectEvaluator m_Evaluator;

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
            m_Evaluator = new RecurringObjectEvaluator(this);
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

        virtual public IList<IRecurrenceDate> ExceptionDates
        {
            get { return Properties.GetList<IRecurrenceDate>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetList<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IList<IRecurrenceDate> RecurrenceDates
        {
            get { return Properties.GetList<IRecurrenceDate>("RDATE"); }
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
