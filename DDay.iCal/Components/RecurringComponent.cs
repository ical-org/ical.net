using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DDay.Collections;

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
#if !SILVERLIGHT
    [Serializable]
#endif
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

        #region Protected Properties

        virtual protected bool EvaluationIncludesReferenceDate { get { return false; } }

        #endregion

        #region Public Properties

        virtual public IList<IAttachment> Attachments
        {
            get { return Properties.GetMany<IAttachment>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

        virtual public IList<string> Categories
        {
            get { return Properties.GetMany<string>("CATEGORIES"); }
            set { Properties.Set("CATEGORIES", value); }
        }

        virtual public string Class
        {
            get { return Properties.Get<string>("CLASS"); }
            set { Properties.Set("CLASS", value); }
        }

        virtual public IList<string> Contacts
        {
            get { return Properties.GetMany<string>("CONTACT"); }
            set { Properties.Set("CONTACT", value); }
        }

        virtual public IDateTime Created
        {
            get { return Properties.Get<IDateTime>("CREATED"); }
            set { Properties.Set("CREATED", value); }
        }

        virtual public string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

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
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IDateTime LastModified
        {
            get { return Properties.Get<IDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }

        virtual public int Priority
        {
            get { return Properties.Get<int>("PRIORITY"); }
            set { Properties.Set("PRIORITY", value); }
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

        virtual public IList<string> RelatedComponents
        {
            get { return Properties.GetMany<string>("RELATED-TO"); }
            set { Properties.Set("RELATED-TO", value); }
        }

        virtual public int Sequence
        {
            get { return Properties.Get<int>("SEQUENCE"); }
            set { Properties.Set("SEQUENCE", value); }
        }

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        virtual public IDateTime Start
        {
            get { return DTStart; }
            set { DTStart = value; }
        }

        virtual public string Summary
        {
            get { return Properties.Get<string>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
        virtual public ICalendarObjectList<IAlarm> Alarms
        {
            get { return new CalendarObjectListProxy<IAlarm>(Children); }
        }

        #endregion

        #region Constructors

        public RecurringComponent() : base()
        {
            Initialize();
            EnsureProperties();
        }

        public RecurringComponent(string name) : base(name)
        {
            Initialize();
            EnsureProperties();
        }

        private void Initialize()
        {
            SetService(new RecurringEvaluator(this));
        }

        #endregion   
     
        #region Private Methods

        private void EnsureProperties()
        {
            if (!Properties.ContainsKey("SEQUENCE"))
                Sequence = 0;
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion

        #region IRecurringComponent Members

        virtual public void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        virtual public IList<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, EvaluationIncludesReferenceDate);
        }

        virtual public IList<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(dt), EvaluationIncludesReferenceDate);
        }

        virtual public IList<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, EvaluationIncludesReferenceDate);
        }

        virtual public IList<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(startTime), new iCalDateTime(endTime), EvaluationIncludesReferenceDate);
        }

        virtual public IList<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null, null);
        }

        virtual public IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime)
        {
            List<AlarmOccurrence> occurrences = new List<AlarmOccurrence>();
            if (Alarms != null)
            {
                foreach (IAlarm alarm in Alarms)
                    occurrences.AddRange(alarm.Poll(startTime, endTime));
            }
            return occurrences;
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
