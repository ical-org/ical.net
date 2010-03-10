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
            get { return Properties.Get<IList<IRecurrenceDate>>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.Get<IList<IRecurrencePattern>>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IList<IRecurrenceDate> RecurrenceDates
        {
            get { return Properties.Get<IList<IRecurrenceDate>>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        virtual public IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.Get<IList<IRecurrencePattern>>("RRULE"); }
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

        #region Internal Properties

        virtual internal iCalDateTime EvalStart
        {
            get { return m_EvalStart; }
            set { m_EvalStart = value; }
        }

        virtual internal iCalDateTime EvalEnd
        {
            get { return m_EvalEnd; }
            set { m_EvalEnd = value; }
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

        #region Internal Methods

        /// <summary>
        /// Evaluates this item to determine the dates and times for which it occurs/recurs.
        /// This method only evaluates items which occur/recur between <paramref name="FromDate"/>
        /// and <paramref name="ToDate"/>; therefore, if you require a list of items which
        /// occur outside of this range, you must specify a <paramref name="FromDate"/> and
        /// <paramref name="ToDate"/> which encapsulate the date(s) of interest.
        /// <note type="caution">
        ///     For events with very complex recurrence rules, this method may be a bottleneck
        ///     during processing time, especially when this method is called for a large number
        ///     of items, in sequence, or for a very large time span.
        /// </note>
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        /// <returns>
        ///     A <see cref="List<Period>"/> containing a <see cref="Period"/> object for
        ///     each date/time this item occurs/recurs.
        /// </returns>
        virtual internal List<Period> Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((!EvalStart.IsAssigned && !EvalEnd.IsAssigned) ||
                (ToDate == EvalStart) || 
                (FromDate == EvalEnd))
            {
                EvaluateRRule(FromDate, ToDate);
                EvaluateRDate(FromDate, ToDate);
                EvaluateExRule(FromDate, ToDate);
                EvaluateExDate(FromDate, ToDate);
                if (!EvalStart.IsAssigned || EvalStart > FromDate)
                    EvalStart = FromDate;
                if (!EvalEnd.IsAssigned || EvalEnd < ToDate)
                    EvalEnd = ToDate;
            }

            if (EvalStart.IsAssigned && FromDate < EvalStart)
                Evaluate(FromDate, EvalStart);
            if (EvalEnd.IsAssigned && ToDate > EvalEnd)
                Evaluate(EvalEnd, ToDate);

            Periods.Sort();

            for (int i = 0; i < Periods.Count; i++)
            {
                Period p = Periods[i];

                // Ensure the Kind of time is consistent with DTStart
                iCalDateTime startTime = p.StartTime;
                startTime.IsUniversalTime = DTStart.IsUniversalTime;
                p.StartTime = startTime;

                Periods[i] = p;
            }

            return Periods;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period
        /// to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle RRULEs
            if (RecurrenceRules != null)
            {
                foreach (IRecurrencePattern rrule in RecurrenceRules)
                {
                    // Get a list of static occurrences
                    // This is important to correctly calculate
                    // recurrences with COUNT.
                    rrule.StaticOccurrences = new List<iCalDateTime>();
                    foreach(Period p in Periods)
                        rrule.StaticOccurrences.Add(p.StartTime);

                    //
                    // Determine the last allowed date in this recurrence
                    //
                    if (rrule.Until.IsAssigned && (!m_Until.IsAssigned || m_Until < rrule.Until))
                        m_Until = rrule.Until;

                    IList<iCalDateTime> DateTimes = rrule.Evaluate(DTStart, FromDate, ToDate);
                    for (int i = 0; i < DateTimes.Count; i++)
                    {
                        iCalDateTime newDt = new iCalDateTime(DateTimes[i]);
                        newDt.TZID = Start.TZID;

                        DateTimes[i] = newDt;
                        Period p = new Period(newDt);

                        if (!Periods.Contains(p))
                        {
                            this.Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the RDate component, and adds each specified DateTime or
        /// Period to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRDate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle RDATEs
            if (RecurrenceDates != null)
            {
                foreach (IRecurrenceDate rdate in RecurrenceDates)
                {
                    IList<Period> periods = rdate.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Period p in periods)
                    {
                        if (!Periods.Contains(p))
                        {
                            Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime
        /// from the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle EXRULEs
            if (ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in ExceptionRules)
                {
                    IList<iCalDateTime> dateTimes = exrule.Evaluate(DTStart, FromDate, ToDate);
                    for (int i = 0; i < dateTimes.Count; i++)
                    {
                        iCalDateTime dt = dateTimes[i];
                        dt.TZID = Start.TZID;
                        dateTimes[i] = dt;

                        Period p = new Period(dt);
                        if (this.Periods.Contains(p))
                            this.Periods.Remove(p);
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or
        /// Period from the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExDate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle EXDATEs
            if (ExceptionDates != null)
            {
                foreach (IRecurrenceDate exdate in ExceptionDates)
                {
                    IList<Period> periods = exdate.Evaluate(DTStart, FromDate, ToDate);
                    for (int i = 0; i < periods.Count; i++)
                    {
                        Period p = periods[i];                        

                        // If no time was provided for the ExDate, then it excludes the entire day
                        if (!p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime))
                            p.MatchesDateOnly = true;

                        while (Periods.Contains(p))
                            Periods.Remove(p);
                    }
                }
            }
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
            EvalStart = DateTime.MaxValue;
            EvalEnd = DateTime.MinValue;
            Periods.Clear();
        }

        virtual public IList<Occurrence> GetOccurrences(iCalDateTime dt)
        {
            return GetOccurrences(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        virtual public IList<Occurrence> GetOccurrences(iCalDateTime startTime, iCalDateTime endTime)
        {
            List<Occurrence> occurrences = new List<Occurrence>();
            List<Period> periods = Evaluate(startTime, endTime);

            foreach (Period p in periods)
            {
                if (p.StartTime >= startTime &&
                    p.StartTime <= endTime)
                    occurrences.Add(new Occurrence(this, p));
            }

            occurrences.Sort();
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
