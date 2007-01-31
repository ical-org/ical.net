using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal.Objects;
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

        private Date_Time m_Due;
        private Duration m_Duration;
        private bool m_Loaded = false;
        private TodoStatus m_Status;

        #endregion

        #region Public Fields
               
        [Serialized, DefaultValueType("DATE-TIME")]
        public Date_Time Completed;        
        [Serialized]
        public Geo Geo;
        [Serialized]
        public Text Location;        
        [Serialized]
        public Integer PercentComplete;        
        [Serialized]
        public TextCollection[] Resources;        

        #endregion

        #region Public Properties

        /// <summary>
        /// The start date/time of the todo item.
        /// </summary>
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
            this.Name = "VTODO";
        }

        #endregion

        #region Static Public Methods

        static public Todo Create(iCalendar iCal)
        {
            Todo t = (Todo)iCal.Create(iCal, "VTODO");
            t.UID = UniqueComponent.NewUID();
            t.Created = DateTime.Now;
            t.DTStamp = DateTime.Now;

            return t;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Use this method to determine if a todo item has been completed.
        /// This takes into account recurrence items and the previous date
        /// of completion, if any.
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
            // Add the todo itself, before recurrence rules are evaluated
            if (DTStart != null)
                Periods.Add(new Period(DTStart));

            return base.Evaluate(FromDate, ToDate);
        }

        /// <summary>
        /// Automatically derives property values based on others it
        /// contains, to provide a more "complete" object.
        /// </summary>
        /// <param name="e"></param>        
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_Loaded = true;            
        }

        /// <summary>
        /// Returns a typed copy of the Todo object.
        /// </summary>
        /// <returns>A typed copy of the Todo object.</returns>
        public Todo Copy()
        {
            return (Todo)base.Copy();
        }

        #endregion

        #region Private Methods

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
