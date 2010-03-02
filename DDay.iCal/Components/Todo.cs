using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal;
using DDay.iCal;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VTODO component.
    /// </summary> 
    [DebuggerDisplay("{Summary} - {Status}")]
#if DATACONTRACT
    [DataContract(Name = "Todo", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Todo : RecurringComponent
    {
        #region Private Fields

        private iCalDateTime m_Completed;
        private iCalDateTime m_Due;
        private Duration m_Duration;
        private Geo m_Geo;
        private bool m_Loaded = false;
        private Text m_Location;
        private Integer m_Percent_Complete;
        private TextCollection[] m_Resources;
        private TodoStatus m_Status;

        #endregion

        #region Public Properties

        /// <summary>
        /// The date/time the todo was completed.
        /// </summary>
        virtual public iCalDateTime Completed
        {
            get { return Properties.Get<iCalDateTime>("COMPLETED"); }
            set { Properties.Set("COMPLETED", value); }
        }

        /// <summary>
        /// The start date/time of the todo item.
        /// </summary>
        public override iCalDateTime DTStart
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
        virtual public iCalDateTime Due
        {
            get { return Properties.Get<iCalDateTime>("DUE"); }
            set
            {
                Properties.Set("DUE", value);
                ExtrapolateTimes();
            }
        }

        /// <summary>
        /// The duration of the todo item.
        /// </summary>
        // NOTE: Duration is not supported by all systems,
        // (i.e. iPhone) and cannot co-exist with Due.
        // RFC 5545 states:
        //
        //      ; either 'due' or 'duration' may appear in
        //      ; a 'todoprop', but 'due' and 'duration'
        //      ; MUST NOT occur in the same 'todoprop'
        //
        // Therefore, Duration is not serialized, as Due
        // should always be extrapolated from the duration.
        virtual public Duration Duration
        {
            get { return Properties.Get<Duration>("DURATION"); }
            set
            {
                Properties.Set("DURATION", value);
                ExtrapolateTimes();
            }
        }

        virtual public Geo Geo
        {
            get { return Properties.Get<Geo>("GEO"); }
            set { Properties.Set("GEO", value); }
        }

        virtual public Text Location
        {
            get { return Properties.Get<Text>("LOCATION"); }
            set { Properties.Set("LOCATION", value); }
        }

        virtual public Integer PercentComplete
        {
            get { return Properties.Get<Integer>("PERCENT-COMPLETE"); }
            set { Properties.Set("PERCENT-COMPLETE", value); }
        }

        virtual public TextCollection[] Resources
        {
            get { return Properties.Get<TextCollection[]>("RESOURCES"); }
            set { Properties.Set("RESOURCES", value); }
        }

        /// <summary>
        /// The status of the todo item.
        /// </summary>
        virtual public TodoStatus Status
        {
            get { return Properties.Get<TodoStatus>("STATUS"); }
            set
            {
                if (Status != value)
                {
                    // Automatically set/unset the Completed time, once the
                    // component is fully loaded (When deserializing, it shouldn't
                    // automatically set the completed time just because the
                    // status was changed).
                    if (IsLoaded)
                    {
                        if (value == TodoStatus.Completed)
                            Completed = DateTime.Now;
                        else Completed = null;
                    }

                    Properties.Set("STATUS", value);
                }
            }
        }

        #endregion

        #region Constructors

        public Todo()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Name = ComponentFactory.TODO;
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
        virtual public bool IsCompleted(iCalDateTime currDt)
        {
            if (Status == TodoStatus.Completed)
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
        virtual public bool IsActive(iCalDateTime currDt)
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
        virtual public bool IsCancelled()
        {
            return Status == TodoStatus.Cancelled;
        }

        virtual public void AddResource(string resource)
        {
            Text r = resource;
            if (Resources != null)
            {
                foreach (TextCollection tc in Resources)
                {
                    if (tc.Values.Contains(r))
                    {
                        return;
                    }
                }
            }

            if (Resources == null ||
                Resources.Length == 0)
            {
                Resources = new TextCollection[1] { new TextCollection(resource) };
                Resources[0].Name = "RESOURCES";
            }
            else
            {
                Resources[0].Values.Add(r);
            }
        }

        virtual public void RemoveResource(string resource)
        {
            if (Resources != null)
            {
                Text r = resource;
                foreach (TextCollection tc in Resources)
                {
                    if (tc.Values.Contains(r))
                    {
                        tc.Values.Remove(r);
                        return;
                    }
                }
            }
        }

        #endregion

        #region Overrides

        internal override List<Period> Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // TODO items can only recur if a start date is specified
            if (DTStart != null)
            {
                // Add the todo itself, before recurrence rules are evaluated
                Period startPeriod = new Period(DTStart);
                if (DTStart != null &&
                    !Periods.Contains(startPeriod))
                    Periods.Add(startPeriod);

                base.Evaluate(FromDate, ToDate);

                // Ensure each period has a duration
                foreach (Period p in Periods)
                {
                    if (p.EndTime == null)
                    {
                        p.Duration = Duration;
                        if (p.Duration != null)
                            p.EndTime = p.StartTime + Duration;
                        else p.EndTime = p.StartTime;
                    }
                    // Ensure the Kind of time is consistent with DTStart
                    else p.EndTime.IsUniversalTime = DTStart.IsUniversalTime;
                }

                return Periods;
            }
            return new List<Period>();
        }

        #endregion

        #region Private Methods

        private void EvaluateToPreviousOccurrence(iCalDateTime completedDate, iCalDateTime currDt)
        {
            iCalDateTime beginningDate = completedDate.Copy<iCalDateTime>();
            if (RRule != null) foreach (RecurrencePattern rrule in RRule) DetermineStartingRecurrence(rrule, ref beginningDate);
            if (RDate != null) foreach (RecurrenceDates rdate in RDate) DetermineStartingRecurrence(rdate, ref beginningDate);
            if (ExRule != null) foreach (RecurrencePattern exrule in ExRule) DetermineStartingRecurrence(exrule, ref beginningDate);
            if (ExDate != null) foreach (RecurrenceDates exdate in ExDate) DetermineStartingRecurrence(exdate, ref beginningDate);

            Evaluate(beginningDate, currDt);
        }

        private void DetermineStartingRecurrence(RecurrenceDates rdate, ref iCalDateTime dt)
        {
            foreach (Period p in rdate.Periods)
            {
                if (p.StartTime < dt)
                    dt = p.StartTime.Copy<iCalDateTime>();
            }
        }

        private void DetermineStartingRecurrence(RecurrencePattern recur, ref iCalDateTime dt)
        {
            if (recur.Count != int.MinValue)
                dt = DTStart.Copy<iCalDateTime>();
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
