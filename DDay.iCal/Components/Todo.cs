using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VTODO component.
    /// </summary> 
    [DebuggerDisplay("{Summary} - {Status}")]
    public class Todo : RecurringComponent
    {
        #region Private Fields

        private Date_Time m_Completed;
        private Date_Time m_Due;
        private Duration m_Duration;
        private Geo m_Geo;
        private bool m_Loaded = false;
        private Text m_Location;
        private Integer m_PercentComplete;
        private TextCollection[] m_Resources;
        private TodoStatus m_Status;

        #endregion

        #region Public Properties

        [Serialized, DefaultValueType("DATE-TIME")]
        public Date_Time Completed
        {
            get { return m_Completed; }
            set { m_Completed = value; }
        }

        /// <summary>
        /// The start date/time of the todo item.
        /// </summary>
        [Serialized, DefaultValueType("DATE-TIME")]
        public override Date_Time DTStart
        {
            get
            {
                return base.DTStart;
            }
            set
            {
                base.DTStart = value;
                ExtrapolateTimes();
            }
        }

        /// <summary>
        /// The due date of the todo item.
        /// </summary>
        [Serialized, DefaultValueType("DATE-TIME")]
        virtual public Date_Time Due
        {
            get { return m_Due; }
            set
            {
                m_Due = value;
                ExtrapolateTimes();
            }
        }

        /// <summary>
        /// The duration of the todo item.
        /// </summary>
        [Serialized, DefaultValue("P")]
        virtual public Duration Duration
        {
            get { return m_Duration; }
            set
            {
                m_Duration = value;
                ExtrapolateTimes();
            }
        }

        [Serialized]
        public Geo Geo
        {
            get { return m_Geo; }
            set { m_Geo = value; }
        }

        [Serialized]
        public Text Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        [Serialized]
        public Integer PercentComplete
        {
            get { return m_PercentComplete; }
            set { m_PercentComplete = value; }
        }

        [Serialized]
        public TextCollection[] Resources
        {
            get { return m_Resources; }
            set { m_Resources = value; }
        }

        /// <summary>
        /// The status of the todo item.
        /// </summary>
        [Serialized, DefaultValue("NEEDS_ACTION\r\n")]
        virtual public TodoStatus Status
        {
            get { return m_Status; }
            set
            {
                if (m_Status != value)
                {
                    // Automatically set/unset the Completed time, once the
                    // component is fully loaded (When deserializing, it shouldn't
                    // automatically track the completed time just because the
                    // status was changed).
                    if (m_Loaded)
                    {
                        if (value == TodoStatus.COMPLETED)
                            Completed = DateTime.Now;
                        else Completed = null;
                    }

                    m_Status = value;
                }
            }
        }

        #endregion

        #region Constructors

        public Todo(iCalObject parent)
            : base(parent)
        {
            this.Name = ComponentBase.TODO;
        }

        #endregion

        #region Static Public Methods

        static public Todo Create(iCalendar iCal)
        {
            Todo t = iCal.Create<Todo>();
            return t;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Use this method to determine if a todo item has been completed.
        /// This takes into account recurrence items and the previous date
        /// of completion, if any.        
        /// <note>
        /// This method evaluates the recurrence pattern for this TODO
        /// as necessary to ensure all relevant information is taken
        /// into account to give the most accurate result possible.
        /// </note>
        /// </summary>
        /// <param name="DateTime">The date and time to test.</param>
        /// <returns>True if the todo item has been completed</returns>
        public bool IsCompleted(Date_Time currDt)
        {
            if (Status == TodoStatus.COMPLETED)
            {
                if (Completed == null ||
                    Completed > currDt)
                    return true;

                EvaluateToPreviousOccurrence(Completed, currDt);

                foreach (Period p in Periods)
                {
                    if (p.StartTime > Completed && // The item has recurred after it was completed
                        currDt >= p.StartTime)     // and the current date is after or on the recurrence date.
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns 'True' if the todo item is Active as of <paramref name="currDt"/>.
        /// An item is Active if it requires action of some sort.
        /// </summary>
        /// <param name="currDt">The date and time to test.</param>
        /// <returns>True if the item is Active as of <paramref name="currDt"/>, False otherwise.</returns>
        public bool IsActive(Date_Time currDt)
        {
            if (DTStart == null)
                return !IsCompleted(currDt) && !IsCancelled();
            else if (currDt >= DTStart)
                return !IsCompleted(currDt) && !IsCancelled();
            else return false;
        }

        /// <summary>
        /// Returns True if the todo item was cancelled.
        /// </summary>
        /// <returns>True if the todo was cancelled, False otherwise.</returns>
        public bool IsCancelled()
        {
            return Status == TodoStatus.CANCELLED;
        }

        #endregion

        #region Overrides

        public override List<Period> Evaluate(Date_Time FromDate, Date_Time ToDate)
        {
            // TODO items can only recur if a start date is specified
            if (DTStart != null)
            {
                // Add the todo itself, before recurrence rules are evaluated
                Period startPeriod = new Period(DTStart);
                if (DTStart != null &&
                    !Periods.Contains(startPeriod))
                    Periods.Add(startPeriod);

                return base.Evaluate(FromDate, ToDate);
            }
            return new List<Period>();
        }

        /// <summary>
        /// Automatically derives property values based on others it
        /// contains, to provide a more "complete" object.
        /// </summary>
        /// <param name="e"></param>        
        public override void OnLoaded(EventArgs e)
        {
            base.OnLoaded(e);
            m_Loaded = true;            
        }
         
        /// <summary>
        /// Returns a typed copy of the Todo object.
        /// </summary>
        /// <returns>A typed copy of the Todo object.</returns>
        public new Todo Copy()
        {
            return (Todo)base.Copy();
        }

        #endregion

        #region Private Methods

        private void EvaluateToPreviousOccurrence(Date_Time completedDate, Date_Time currDt)
        {
            Date_Time beginningDate = completedDate.Copy();
            if (RRule != null) foreach (Recur rrule in RRule) DetermineStartingRecurrence(rrule, ref beginningDate);
            if (RDate != null) foreach (RDate rdate in RDate) DetermineStartingRecurrence(rdate, ref beginningDate);
            if (ExRule != null) foreach (Recur exrule in ExRule) DetermineStartingRecurrence(exrule, ref beginningDate);
            if (ExDate != null) foreach (RDate exdate in ExDate) DetermineStartingRecurrence(exdate, ref beginningDate);

            Evaluate(beginningDate, currDt);
        }

        private void DetermineStartingRecurrence(RDate rdate, ref Date_Time dt)
        {
            foreach (Period p in rdate.Periods)
            {
                if (p.StartTime < dt)
                    dt = p.StartTime.Copy();
            }
        }

        private void DetermineStartingRecurrence(Recur recur, ref Date_Time dt)
        {
            if (recur.Count != int.MinValue)
                dt = DTStart.Copy();
            else recur.IncrementDate(ref dt, -recur.Interval);
        }

        private void ExtrapolateTimes()
        {
            if (Due == null && DTStart != null && Duration != null)
                Due = DTStart + Duration;
            else if (Duration == null && DTStart != null && Due != null)
                Duration = Due - DTStart;
            else if (DTStart == null && Duration != null && Due != null)
                DTStart = Due - Duration;            
        }

        #endregion
    }
}
