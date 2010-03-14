using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using DDay.iCal;
using DDay.iCal.Serialization;
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

        private iCalDateTime m_EvalStart;
        private iCalDateTime m_EvalEnd;
        private iCalDateTime m_Until;

        private List<Period> m_Periods;
        private IList<IAlarm> m_Alarms;

        #endregion

        #region Public Properties

        /// <summary>
        /// The start date/time of the component.
        /// </summary>
        virtual public iCalDateTime DTStart
        {
            get { return Properties.Get<iCalDateTime>("DTSTART"); }            
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

        virtual public iCalDateTime RecurrenceID
        {
            get { return Properties.Get<iCalDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        virtual public iCalDateTime Start
        {
            get { return DTStart; }
            set { DTStart = value; }
        }

        /// <summary>
        /// A collection of <see cref="Period"/> objects that contain the dates and times
        /// when each item occurs/recurs.
        /// </summary>
        virtual protected List<Period> Periods
        {
            get { return m_Periods; }
            set { m_Periods = value; }
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
            Periods = new List<Period>();
            Alarms = new List<IAlarm>();

            RecurrenceDates = new List<IRecurrenceDate>();
            RecurrenceRules = new List<IRecurrencePattern>();
            ExceptionDates = new List<IRecurrenceDate>();
            ExceptionRules = new List<IRecurrencePattern>();
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

        #endregion

        #region IRecurringComponent Members

        virtual public void ClearEvaluation()
        {
            IPeriodEvaluator evaluator = GetService(typeof(IPeriodEvaluator)) as IPeriodEvaluator;
            if (evaluator != null)
                evaluator.Clear();
        }

        virtual public IList<Occurrence> GetOccurrences(iCalDateTime dt)
        {
            return GetOccurrences(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        virtual public IList<Occurrence> GetOccurrences(iCalDateTime startTime, iCalDateTime endTime)
        {
            List<Occurrence> occurrences = new List<Occurrence>();

            IPeriodEvaluator evaluator = GetService(typeof(IPeriodEvaluator)) as IPeriodEvaluator;
            if (evaluator != null)
            {
                IList<Period> periods = evaluator.Evaluate(Start, startTime, endTime);

                foreach (Period p in periods)
                {
                    // Filter the resulting periods to only contain those between
                    // startTime and endTime.
                    if (p.StartTime >= startTime &&
                        p.StartTime <= endTime)
                        occurrences.Add(new Occurrence(this, p));
                }

                occurrences.Sort();
            }
            return occurrences;
        }

        virtual public IList<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(default(iCalDateTime), default(iCalDateTime));
        }

        virtual public IList<AlarmOccurrence> PollAlarms(iCalDateTime Start, iCalDateTime End)
        {
            List<AlarmOccurrence> Occurrences = new List<AlarmOccurrence>();
            if (Alarms != null)
            {
                foreach (IAlarm alarm in Alarms)
                    Occurrences.AddRange(alarm.Poll(Start, End));
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
