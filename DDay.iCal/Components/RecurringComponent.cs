using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// An iCalendar component that recurs.
    /// </summary>
    /// <remarks>
    /// This component automatically handles
    /// RRULEs, RDATE, EXRULEs, and EXDATEs, as well as the DTSTART
    /// for the recurring item (all recurring items must have a DTSTART).
    /// </remarks>
#if DATACONTRACT
    [DataContract(Name = "RecurringComponent", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class RecurringComponent : 
        UniqueComponent,
        IRecurringComponent
    {
        #region Static Public Methods

        static public IEnumerable<IRecurringComponent> SortByDate(IEnumerable<IRecurringComponent> list)
        {
            return SortByDate<IRecurringComponent>(list);
        }

        static public IEnumerable<T> SortByDate<T>(IEnumerable<T> list)
        {
            List<IRecurringComponent> items = new List<IRecurringComponent>();
            foreach (T t in list)
            {
                if (t is IRecurringComponent)
                    items.Add((IRecurringComponent)(object)t);
            }

            // Sort the list by date
            items.Sort(new RecurringComponentDateSorter());
            foreach (IRecurringComponent rc in items)
                yield return (T)(object)rc;
        }

        #endregion

        #region Private Fields
                
        private IList<IAlarm> m_Alarms;
        private RecurringEvaluator m_Evaluator;

        #endregion

        #region Public Properties

        /// <summary>
        /// The start date/time of the component.
        /// </summary>
        virtual public IDateTime DTStart
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

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        virtual public IDateTime Start
        {
            get { return DTStart; }
            set { DTStart = value; }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public IList<IAlarm> Alarms
        {
            get { return m_Alarms; }
            set { m_Alarms = value; }
        }

        #endregion

        #region Constructors

        public RecurringComponent() : base() { Initialize(); }
        public RecurringComponent(string name) : base(name) { Initialize(); }
        private void Initialize()
        {
            m_Evaluator = new RecurringEvaluator(this);
            Alarms = new List<IAlarm>();
        }

        #endregion        
        
        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void AddChild(ICalendarObject child)
        {
            if (child is IAlarm)
                Alarms.Add((IAlarm)child);
            base.AddChild(child);
        }

        public override void RemoveChild(ICalendarObject child)
        {
            if (child is IAlarm)
                Alarms.Remove((IAlarm)child);
            base.RemoveChild(child);
        }

        public override object GetService(Type serviceType)
        {
            if (typeof(IEvaluator).IsAssignableFrom(serviceType))
                return m_Evaluator;
            return null;
        }

        #endregion

        #region IRecurringComponent Members

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

        virtual public IList<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null, null);
        }

        virtual public IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime)
        {
            List<AlarmOccurrence> Occurrences = new List<AlarmOccurrence>();
            if (Alarms != null)
            {
                foreach (IAlarm alarm in Alarms)
                    Occurrences.AddRange(alarm.Poll(startTime, endTime));
            }
            return Occurrences;
        }

        #endregion
    }

    /// <summary>
    /// Sorts recurring components by their start dates
    /// </summary>
    public class RecurringComponentDateSorter : IComparer<IRecurringComponent>
    {
        #region IComparer<RecurringComponent> Members

        public int Compare(IRecurringComponent x, IRecurringComponent y)
        {
            return x.Start.CompareTo(y.Start);            
        }

        #endregion
    }
}
