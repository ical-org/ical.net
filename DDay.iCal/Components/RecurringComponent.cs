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

        private Date_Time m_DTStart;
        private Date_Time m_EvalStart;
        private Date_Time m_EvalEnd;
        private Date_Time m_Until;
        private RDate[] m_ExDate;
        private Recur[] m_ExRule;
        private RDate[] m_RDate;
        private Recur[] m_RRule;
        private Date_Time m_RecurID;

        private List<Period> m_Periods;
        private List<Alarm> m_Alarms;

        private bool m_IsOriginal = true;        
        private RecurringComponent m_Original = null;        

        #endregion

        #region Public Properties

        /// <summary>
        /// The start date/time of the component.
        /// </summary>
        [Serialized, DefaultValueType("DATE-TIME")]
        virtual public Date_Time DTStart
        {
            get { return m_DTStart; }
            set { m_DTStart = value; }
        }

        public Date_Time EvalStart
        {
            get { return m_EvalStart; }
            set { m_EvalStart = value; }
        }

        public Date_Time EvalEnd
        {
            get { return m_EvalEnd; }
            set { m_EvalEnd = value; }
        }

        [Serialized]
        public RDate[] ExDate
        {
            get { return m_ExDate; }
            set { m_ExDate = value; }
        }

        [Serialized]
        public Recur[] ExRule
        {
            get { return m_ExRule; }
            set { m_ExRule = value; }
        }

        /// <summary>
        /// A collection of <see cref="Period"/> objects that contain the dates and times
        /// when each item occurs/recurs.
        /// </summary>
        virtual public List<Period> Periods
        {
            get { return m_Periods; }
            set { m_Periods = value; }
        }

        [Serialized]
        public RDate[] RDate
        {
            get { return m_RDate; }
            set { m_RDate = value; }
        }

        [Serialized]
        public Date_Time RecurID
        {
            get { return m_RecurID; }
            set { m_RecurID = value; }
        }

        [Serialized]
        public Recur[] RRule
        {
            get { return m_RRule; }
            set { m_RRule = value; }
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
            get { return m_Until; }
            set { m_Until = value; }
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
        virtual public List<Alarm> Alarms
        {
            get { return m_Alarms; }
            set { m_Alarms = value; }
        }

        /// <summary>
        /// Returns true if the component is a original object.
        /// A flattened instance of a component (one returned by
        /// FlattenXXX() methods) are not original, but are copies
        /// of an original object.
        /// </summary>
        public bool IsOriginal
        {
            get { return m_IsOriginal; }            
        }

        /// <summary>
        /// Returns the original object for this component.
        /// If this component represents a single recurrence instance
        /// of another component, this will return the original
        /// component which generated the current instance.  If
        /// the current component is the original component, <code>this</code>
        /// will be returned.
        /// </summary>
        public RecurringComponent Original
        {
            get
            {
                // Find the original component
                RecurringComponent rc = this;
                while (rc.m_Original != null)
                    rc = rc.m_Original;

                return rc;                
            }
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

        // FIXME: add similar methods for RDATE and EXDATE

        /// <summary>
        /// Adds a recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        public void AddRecurrence(Recur recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "RRULE";

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
        /// Adds an exception recurrence rule to the recurring component
        /// </summary>
        /// <param name="recur">The recurrence rule to add</param>
        public void AddException(Recur recur)
        {
            if (string.IsNullOrEmpty(recur.Name))
                recur.Name = "EXRULE";

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
        /// "Flattens" a single component recurrence into a copy of the
        /// component.  This essentially "extracts" a recurrence into
        /// a fully-fledged non-recurring object (a single instance).
        /// </summary>
        /// <param name="obj">The iCalObject that will contain this recurring component</param>
        /// <param name="p">The period (recurrence instance) to be flattened</param>
        /// <returns>A recurring component which represents a single flattened recurrence instance</returns>
        virtual protected RecurringComponent FlattenInstance(iCalObject obj, Period p)
        {            
            // Copy the component into the dummy iCalendar
            RecurringComponent rc = (RecurringComponent)Copy(obj);

            rc.m_IsOriginal = false;
            rc.m_Original = this;
            rc.Start = p.StartTime.Copy();
            rc.RRule = new Recur[0];
            rc.RDate = new RDate[0];
            rc.ExRule = new Recur[0];
            rc.ExDate = new RDate[0];

            return rc;
        }

        /// <summary>
        /// "Flattens" component recurrences into a series of equivalent objects.
        /// </summary>        
        /// <returns>A list of <see cref="Event"/>s if they could be flattened, null otherwise.</returns>
        virtual public IEnumerable<RecurringComponent> FlattenRecurrences()
        {
            // Create a dummy iCalendar to hold our flattened component
            iCalendar iCal = new iCalendar();

            if (Start.TZID != null)
            {
                // Place the time zone into our dummy iCalendar
                DDay.iCal.Components.TimeZone tz = iCalendar.GetTimeZone(Start.TZID);
                if (tz != null)
                    tz.Copy(iCal);
            }

            // Iterate through each period to find all occurrences on this date
            foreach (Period p in Periods)
                yield return FlattenInstance(iCal, p);            
        }

        /// <summary>
        /// "Flattens" component recurrences that occur in the given date into a series of equivalent objects.
        /// </summary>
        /// <param name="dt">The date on which the event recurs</param>
        /// <returns>A list of <see cref="Event"/>s if they could be flattened, null otherwise.</returns>
        virtual public IEnumerable<RecurringComponent> FlattenRecurrencesOn(Date_Time dt)
        {            
            // Create a dummy iCalendar to hold our flattened component
            iCalendar iCal = new iCalendar();

            if (Start.TZID != null)
            {
                // Place the time zone into our dummy iCalendar
                DDay.iCal.Components.TimeZone tz = iCalendar.GetTimeZone(Start.TZID);
                if (tz != null)
                    tz.Copy(iCal);
            }

            // Iterate through each period to find all occurrences on this date
            foreach (Period p in Periods)
            {
                // Check to see if this occurrence is on the same date
                if (p.StartTime.Date.Equals(dt.Date))
                {
                    // Copy the component into the dummy iCalendar
                    RecurringComponent rc = (RecurringComponent)Copy(iCal);

                    rc.Start = p.StartTime.Copy();
                    rc.RRule = new Recur[0];
                    rc.RDate = new RDate[0];
                    rc.ExRule = new Recur[0];
                    rc.ExDate = new RDate[0];

                    yield return rc;
                }
            }
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
                {
                    p.StartTime.Value = DateTime.SpecifyKind(p.StartTime.Value, DTStart.Kind);
                }
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
        /// All events starting between 12:00:00AM and 11:59:59 PM will be
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
        /// All events occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
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
        virtual public List<Alarm.AlarmOccurrence> PollAlarms()
        {
            return PollAlarms(null);
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
        /// // Evaluate this recurring event for the month of January, 2006.
        /// evt.Evaluate(
        ///     new Date_Time(2006, 1, 1, "US-Eastern", iCal),
        ///     new Date_Time(2006, 1, 31, "US-Eastern", iCal));
        /// 
        /// // Poll all alarms that occurred in January, 2006.
        /// List<Alarm.AlarmOccurrence> alarms = evt.PollAlarms();
        /// 
        /// // Here, you would eliminate alarms that the user has already dismissed.
        /// // This information should be stored somewhere outside of the .ics file.
        /// // You can use the component's UID, and the AlarmOccurence date/time 
        /// // as the primary key for each alarm occurrence.
        /// 
        /// foreach(Alarm.AlarmOccurrence alarm in alarms)        
        ///     MessageBox.Show(alarm.Component.Summary + "\n" + alarm.Alarm.DateTime);        
        /// </code>
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="Start">The earliest allowable alarm occurrence to poll, or <c>null</c>.</param>
        /// <returns>A List of <see cref="Alarm.AlarmOccurrence"/> objects, one for each occurrence of the <see cref="Alarm"/>.</returns>
        virtual public List<Alarm.AlarmOccurrence> PollAlarms(Date_Time Start)
        {
            List<Alarm.AlarmOccurrence> Occurrences = new List<Alarm.AlarmOccurrence>();
            foreach (Alarm alarm in Alarms)
                Occurrences.AddRange(alarm.Poll(Start));
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
