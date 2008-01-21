using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// An iCalendar component that recurs.
    /// </summary>
    /// <remarks>
    /// This component automatically handles
    /// RRULEs, RDATE, EXRULEs, and EXDATEs, as well as the DTSTART
    /// for the recurring item (all recurring items must have a DTSTART).
    /// </remarks>
    public class RecurringComponent : UniqueComponent
    {
        #region Private Fields

        private Date_Time _DTStart;
        private Date_Time _EvalStart;
        private Date_Time _EvalEnd;
        private Date_Time _Until;
        private RDate[] _ExDate;
        private Recur[] _ExRule;
        private RDate[] _RDate;
        private Recur[] _RRule;
        private Date_Time _RecurID;

        private List<Period> _Periods;
        private List<Alarm> _Alarms;

        #endregion

        #region Public Properties

        /// <summary>
        /// The start date/time of the component.
        /// </summary>
        [Serialized, DefaultValueType("DATE-TIME")]
        virtual public Date_Time DTStart
        {
            get { return _DTStart; }
            set { _DTStart = value; }
        }

        public Date_Time EvalStart
        {
            get { return _EvalStart; }
            set { _EvalStart = value; }
        }

        public Date_Time EvalEnd
        {
            get { return _EvalEnd; }
            set { _EvalEnd = value; }
        }

        [Serialized]
        public RDate[] ExDate
        {
            get { return _ExDate; }
            set { _ExDate = value; }
        }

        [Serialized]
        public Recur[] ExRule
        {
            get { return _ExRule; }
            set { _ExRule = value; }
        }

        /// <summary>
        /// A collection of <see cref="Period"/> objects that contain the dates and times
        /// when each item occurs/recurs.
        /// </summary>
        virtual public List<Period> Periods
        {
            get { return _Periods; }
            set { _Periods = value; }
        }

        [Serialized]
        public RDate[] RDate
        {
            get { return _RDate; }
            set { _RDate = value; }
        }

        [Serialized]
        public Date_Time RecurID
        {
            get { return _RecurID; }
            set { _RecurID = value; }
        }

        [Serialized]
        public Recur[] RRule
        {
            get { return _RRule; }
            set { _RRule = value; }
        }

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        virtual public Date_Time Start
        {
            get { return DTStart; }
            set { DTStart = value; }
        }

        public Date_Time Until
        {
            get { return _Until; }
            set { _Until = value; }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
        virtual public List<Alarm> Alarms
        {
            get { return _Alarms; }
            set { _Alarms = value; }
        }

        #endregion

        #region Constructors

        public RecurringComponent() : base() { Initialize(); }
        public RecurringComponent(iCalObject parent) : base(parent) { Initialize(); }
        public RecurringComponent(iCalObject parent, string name) : base(parent, name) { Initialize(); }
        public void Initialize()
        {
            Periods = new List<Period>();
            Alarms = new List<Alarm>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        public void AddRecurrence(Recur recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "RRULE";

            recur.Parent = this;

            if (RRule != null)
            {
                Recur[] rules = new Recur[RRule.Length + 1];
                RRule.CopyTo(rules, 0);
                rules[rules.Length - 1] = recur;
                RRule = rules;
            }
            else RRule = new Recur[] { recur };
        }

        /// <summary>
        /// Adds a single recurrence for this recurring component.
        /// </summary>
        /// <param name="dt">The date/time when this component will recur.</param>
        public void AddSingleRecurrence(Date_Time dt)
        {
            RDate rdate = new RDate();
            rdate.Name = "RDATE";
            rdate.Add(dt);

            rdate.Parent = this;

            if (RDate != null)
            {
                RDate[] dates = new RDate[RDate.Length + 1];
                RDate.CopyTo(dates, 0);
                dates[dates.Length - 1] = rdate;
                RDate = dates;
            }
            else RDate = new RDate[] { rdate };
        }

        /// <summary>
        /// Adds an exception recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        public void AddException(Recur recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "EXRULE";

            recur.Parent = this;

            if (ExRule != null)
            {
                Recur[] rules = new Recur[ExRule.Length + 1];
                ExRule.CopyTo(rules, 0);
                rules[rules.Length - 1] = recur;
                ExRule = rules;
            }
            else ExRule = new Recur[] { recur };
        }

        /// <summary>
        /// Adds a single exception for this recurring component
        /// </summary>
        /// <param name="recur">The date/time when this component will NOT recur.</param>
        public void AddSingleException(Date_Time dt)
        {
            RDate exdate = new RDate();
            exdate.Name = "EXDATE";
            exdate.Add(dt);

            exdate.Parent = this;

            if (ExDate != null)
            {
                RDate[] dates = new RDate[ExDate.Length + 1];
                ExDate.CopyTo(dates, 0);
                dates[dates.Length - 1] = exdate;
                ExDate = dates;
            }
            else ExDate = new RDate[] { exdate };
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

        #region Public Overridables

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
        virtual public List<Period> Evaluate(Date_Time FromDate, Date_Time ToDate)
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
                    EvalStart = FromDate.Copy();
                if (EvalEnd == null || EvalEnd < ToDate)
                    EvalEnd = ToDate.Copy();
            }

            if (EvalStart != null && FromDate < EvalStart)
                Evaluate(FromDate, EvalStart);
            if (EvalEnd != null && ToDate > EvalEnd)
                Evaluate(EvalEnd, ToDate);

            Periods.Sort();

            foreach(Period p in Periods)
            {
                // Ensure the Kind of time is consistent with DTStart
                if (p.StartTime.Kind != DTStart.Kind)
                    p.StartTime.Kind = DTStart.Kind;
            }
            
            // Evaluate all Alarms for this component.
            foreach (Alarm alarm in Alarms)
                alarm.Evaluate(this);

            return Periods;
        }

        /// <summary>
        /// Clears a previous evaluation, usually because one of the 
        /// key elements used for evaluation has changed 
        /// (Start, End, Duration, recurrence rules, exceptions, etc.).
        /// </summary>
        virtual public void ClearEvaluation()
        {
            EvalStart = null;
            EvalEnd = null;
            Periods.Clear();

            foreach (Alarm alarm in Alarms)
                alarm.Occurrences.Clear();
        }

        /// <summary>
        /// Returns all occurrences of this component that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        virtual public List<Occurrence> GetOccurrences(Date_Time dt)
        {
            return GetOccurrences(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns all occurrences of this component that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        virtual public List<Occurrence> GetOccurrences(Date_Time startTime, Date_Time endTime)
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

        /// <summary>
        /// Polls alarms for the current evaluation period.  This period is defined by the 
        /// range indicated in EvalStart and EvalEnd properties.  These properties are automatically
        /// set when calling the Evaluate() method with a given date range, and indicate the date
        /// range currently "known" by the recurring component.
        /// </summary>
        /// <returns>A list of AlarmOccurrence objects, representing each alarm that has fired.</returns>
        virtual public List<AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null, null);
        }

        /// <summary>
        /// Polls <see cref="Alarm"/>s for occurrences within the <see cref="Evaluate"/>d
        /// time frame of this <see cref="RecurringComponent"/>.  For each evaluated
        /// occurrence if this component, each <see cref="Alarm"/> is polled for its
        /// corresponding alarm occurrences.
        /// <para>
        /// <example>
        /// The following is an example of polling alarms for an event.
        /// <code>
        /// iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// Event evt = iCal.Events[0];
        ///
        /// // Poll the alarms on the event
        /// List<AlarmOccurrence> alarms = evt.PollAlarms();
        /// 
        /// // Here, you would eliminate alarms that the user has already dismissed.
        /// // This information should be stored somewhere outside of the .ics file.
        /// // You can use the component's UID, and the AlarmOccurence date/time 
        /// // as the primary key for each alarm occurrence.
        /// 
        /// foreach(AlarmOccurrence alarm in alarms)
        ///     MessageBox.Show(alarm.Component.Summary + "\n" + alarm.DateTime);
        /// </code>
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="Start">The earliest allowable alarm occurrence to poll, or <c>null</c>.</param>
        /// <returns>A List of <see cref="Alarm.AlarmOccurrence"/> objects, one for each occurrence of the <see cref="Alarm"/>.</returns>
        virtual public List<AlarmOccurrence> PollAlarms(Date_Time Start, Date_Time End)
        {
            List<AlarmOccurrence> Occurrences = new List<AlarmOccurrence>();
            foreach (Alarm alarm in Alarms)
                Occurrences.AddRange(alarm.Poll(Start, End));
            return Occurrences;
        }

        #endregion

        #region Protected Overridables

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period
        /// to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRRule(Date_Time FromDate, Date_Time ToDate)
        {
            // Handle RRULEs
            if (RRule != null)
            {
                foreach (Recur rrule in RRule)
                {
                    // Get a list of static occurrences
                    // This is important to correctly calculate
                    // recurrences with COUNT.
                    rrule.StaticOccurrences = new List<Date_Time>();
                    foreach(Period p in Periods)
                        rrule.StaticOccurrences.Add(p.StartTime);

                    //
                    // Determine the last allowed date in this recurrence
                    //
                    if (rrule.Until != null && (Until == null || Until < rrule.Until))
                        Until = rrule.Until.Copy();

                    List<Date_Time> DateTimes = rrule.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Date_Time dt in DateTimes)
                    {
                        Period p = new Period(dt);

                        if (!Periods.Contains(p))
                            this.Periods.Add(p);
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
        virtual protected void EvaluateRDate(Date_Time FromDate, Date_Time ToDate)
        {
            // Handle RDATEs
            if (RDate != null)
            {
                foreach (RDate rdate in RDate)
                {
                    List<Period> periods = rdate.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Period p in periods)
                    {   
                        if (p != null && !Periods.Contains(p))
                            Periods.Add(p);
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
        virtual protected void EvaluateExRule(Date_Time FromDate, Date_Time ToDate)
        {
            // Handle EXRULEs
            if (ExRule != null)
            {
                foreach (Recur exrule in ExRule)
                {
                    List<Date_Time> DateTimes = exrule.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Date_Time dt in DateTimes)
                    {
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
        virtual protected void EvaluateExDate(Date_Time FromDate, Date_Time ToDate)
        {
            // Handle EXDATEs
            if (ExDate != null)
            {
                foreach (RDate exdate in ExDate)
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

        public override void AddChild(iCalObject child)
        {
            if (child is Alarm)
                Alarms.Add((Alarm)child);
            base.AddChild(child);
        }

        public override void RemoveChild(iCalObject child)
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
