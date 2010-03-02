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
        #region Private Fields

        private iCalDateTime _EvalStart;
        private iCalDateTime _EvalEnd;
        private iCalDateTime _Until;

        private List<Period> _Periods;
        private List<Alarm> _Alarms;

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

        virtual public RecurrenceDates[] ExDate
        {
            get { return Properties.Get<RecurrenceDates[]>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public RecurrencePattern[] ExRule
        {
            get { return Properties.Get<RecurrencePattern[]>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public RecurrenceDates[] RDate
        {
            get { return Properties.Get<RecurrenceDates[]>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        virtual public iCalDateTime RecurrenceID
        {
            get { return Properties.Get<iCalDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        virtual public RecurrencePattern[] RRule
        {
            get { return Properties.Get<RecurrencePattern[]>("RRULE"); }
            set { Properties.Set("RRULE", value); }
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
        /// Gets/sets the UNTIL value that is specified in the
        /// recurrence pattern of the component.  NOTE: This isn't
        /// set until the recurrence pattern is evaluated.
        /// </summary>
        virtual public iCalDateTime Until
        {
            get { return _Until; }
            set
            {
                _Until = value;
                if (_Until != null)
                    _Until.Name = "UNTIL";
            }
        }

        /// <summary>
        /// A collection of <see cref="Period"/> objects that contain the dates and times
        /// when each item occurs/recurs.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public List<Period> Periods
        {
            get { return _Periods; }
            set { _Periods = value; }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public List<Alarm> Alarms
        {
            get { return _Alarms; }
            set { _Alarms = value; }
        }

        #endregion

        #region Internal Properties

        virtual internal iCalDateTime EvalStart
        {
            get { return _EvalStart; }
            set { _EvalStart = value; }
        }

        virtual internal iCalDateTime EvalEnd
        {
            get { return _EvalEnd; }
            set { _EvalEnd = value; }
        }

        #endregion

        #region Constructors

        public RecurringComponent() : base() { Initialize(); }
        public RecurringComponent(string name) : base(name) { Initialize(); }
        private void Initialize()
        {
            Periods = new List<Period>();
            Alarms = new List<Alarm>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an alarm to the recurring component
        /// </summary>        
        virtual public void AddAlarm(Alarm alarm)
        {
            alarm.Name = ComponentFactory.ALARM;            
            AddChild(alarm);
        }        

        /// <summary>
        /// Adds a recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        virtual public void AddRecurrencePattern(RecurrencePattern recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "RRULE";

            recur.Parent = this;

            if (RRule != null)
            {
                RecurrencePattern[] rules = new RecurrencePattern[RRule.Length + 1];
                RRule.CopyTo(rules, 0);
                rules[rules.Length - 1] = recur;
                RRule = rules;
            }
            else RRule = new RecurrencePattern[] { recur };
        }

        /// <summary>
        /// Adds a single recurrence for this recurring component.
        /// </summary>
        /// <param name="dt">The date/time when this component will recur.</param>
        virtual public void AddSingleRecurrence(iCalDateTime dt)
        {
            RecurrenceDates rdate = new RecurrenceDates();
            rdate.Name = "RDATE";
            rdate.Add(dt);

            rdate.Parent = this;

            if (RDate != null &&
                RDate.Length > 0)
            {
                RDate[0].Add(dt);
            }
            else RDate = new RecurrenceDates[] { rdate };
        }

        /// <summary>
        /// Adds a single recurrence for this recurring component.
        /// </summary>
        /// <param name="p">The period of time when this component will recur.</param>
        virtual public void AddSingleRecurrence(Period p)
        {
            RecurrenceDates rdate = new RecurrenceDates();
            rdate.Name = "RDATE";
            rdate.Add(p);

            rdate.Parent = this;

            if (RDate != null &&
                RDate.Length > 0)
            {
                RDate[0].Add(p);
            }
            else RDate = new RecurrenceDates[] { rdate };
        }

        /// <summary>
        /// Adds an exception recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        virtual public void AddExceptionPattern(RecurrencePattern recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "EXRULE";

            recur.Parent = this;

            if (ExRule != null)
            {
                RecurrencePattern[] rules = new RecurrencePattern[ExRule.Length + 1];
                ExRule.CopyTo(rules, 0);
                rules[rules.Length - 1] = recur;
                ExRule = rules;
            }
            else ExRule = new RecurrencePattern[] { recur };
        }

        /// <summary>
        /// Adds a single exception for this recurring component
        /// </summary>
        /// <param name="recur">The date/time when this component will NOT recur.</param>
        virtual public void AddSingleException(iCalDateTime dt)
        {
            RecurrenceDates exdate = new RecurrenceDates();
            exdate.Name = "EXDATE";
            exdate.Add(dt);

            exdate.Parent = this;

            if (ExDate != null)
            {
                RecurrenceDates[] dates = new RecurrenceDates[ExDate.Length + 1];
                ExDate.CopyTo(dates, 0);
                dates[dates.Length - 1] = exdate;
                ExDate = dates;
            }
            else ExDate = new RecurrenceDates[] { exdate };
        }

        #endregion

        #region Static Public Methods

        static public IEnumerable<RecurringComponent> SortByDate(IEnumerable<RecurringComponent> list)
        {
            return SortByDate<RecurringComponent>(list);
        }

        static public IEnumerable<T> SortByDate<T>(IEnumerable<T> list)
        {
            List<RecurringComponent> items = new List<RecurringComponent>();
            foreach (T t in list)
            {
                if (t is RecurringComponent)
                    items.Add((RecurringComponent)(object)t);
            }

            // Sort the list by date
            items.Sort(new RecurringComponentDateSorter());
            foreach (RecurringComponent rc in items)
                yield return (T)(object)rc;
        }

        #endregion

        #region IRecurringComponent Members
                
        virtual public void ClearEvaluation()
        {
            EvalStart = null;
            EvalEnd = null;
            Periods.Clear();
        }

        virtual public List<Occurrence> GetOccurrences(iCalDateTime dt)
        {
            return GetOccurrences(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        virtual public List<Occurrence> GetOccurrences(iCalDateTime startTime, iCalDateTime endTime)
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

        virtual public List<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null, null);
        }
      
        virtual public List<AlarmOccurrence> PollAlarms(iCalDateTime Start, iCalDateTime End)
        {
            List<AlarmOccurrence> Occurrences = new List<AlarmOccurrence>();
            foreach (Alarm alarm in Alarms)
                Occurrences.AddRange(alarm.Poll(Start, End));
            return Occurrences;
        }

        #endregion

        #region Internal Overridables

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
            if ((EvalStart == null && EvalEnd == null) ||
                (ToDate == EvalStart) ||
                (FromDate == EvalEnd))
            {
                EvaluateRRule(FromDate, ToDate);
                EvaluateRDate(FromDate, ToDate);
                EvaluateExRule(FromDate, ToDate);
                EvaluateExDate(FromDate, ToDate);
                if (EvalStart == null || EvalStart > FromDate)
                    EvalStart = FromDate.Copy<iCalDateTime>();
                if (EvalEnd == null || EvalEnd < ToDate)
                    EvalEnd = ToDate.Copy<iCalDateTime>();
            }

            if (EvalStart != null && FromDate < EvalStart)
                Evaluate(FromDate, EvalStart);
            if (EvalEnd != null && ToDate > EvalEnd)
                Evaluate(EvalEnd, ToDate);

            Periods.Sort();

            foreach (Period p in Periods)
            {
                // Ensure the Kind of time is consistent with DTStart
                p.StartTime.IsUniversalTime = DTStart.IsUniversalTime;
            }

            return Periods;
        }

        #endregion

        #region Protected Overridables

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period
        /// to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle RRULEs
            if (RRule != null)
            {
                foreach (RecurrencePattern rrule in RRule)
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
                    if (rrule.Until != null && (Until == null || Until < rrule.Until))
                        Until = rrule.Until.Copy<iCalDateTime>();

                    List<iCalDateTime> DateTimes = rrule.Evaluate(DTStart, FromDate, ToDate);
                    foreach (iCalDateTime dt in DateTimes)
                    {
                        dt.TZID = Start.TZID;
                        Period p = new Period(dt);

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
            if (RDate != null)
            {
                foreach (RecurrenceDates rdate in RDate)
                {
                    List<Period> periods = rdate.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Period p in periods)
                    {
                        if (p != null && !Periods.Contains(p))
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
            if (ExRule != null)
            {
                foreach (RecurrencePattern exrule in ExRule)
                {
                    List<iCalDateTime> DateTimes = exrule.Evaluate(DTStart, FromDate, ToDate);
                    foreach (iCalDateTime dt in DateTimes)
                    {
                        dt.TZID = Start.TZID;

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
            if (ExDate != null)
            {
                foreach (RecurrenceDates exdate in ExDate)
                {
                    List<Period> periods = exdate.Evaluate(DTStart, FromDate, ToDate);
                    foreach(Period p in periods)
                    {
                        // If no time was provided for the ExDate, then it excludes the entire day
                        if (!p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime))
                            p.MatchesDateOnly = true;

                        if (p != null)
                        {
                            while (Periods.Contains(p))
                                Periods.Remove(p);
                        }
                    }
                }
            }
        }

        #endregion

        #region Overrides

        public override void AddChild(ICalendarObject child)
        {
            if (child is Alarm)
                Alarms.Add((Alarm)child);
            base.AddChild(child);
        }

        public override void RemoveChild(ICalendarObject child)
        {
            if (child is Alarm)
                Alarms.Remove((Alarm)child);
            base.RemoveChild(child);
        }

        #endregion
    }

    /// <summary>
    /// Sorts recurring components by their start dates
    /// </summary>
    public class RecurringComponentDateSorter : IComparer<RecurringComponent>
    {
        #region IComparer<RecurringComponent> Members

        public int Compare(RecurringComponent x, RecurringComponent y)
        {
            return x.Start.CompareTo(y.Start);            
        }

        #endregion
    }
}
