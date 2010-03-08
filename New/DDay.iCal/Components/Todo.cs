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
    public class Todo : 
        RecurringComponent,
        ITodo
    {
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
        virtual public TimeSpan Duration
        {
            get { return Properties.Get<TimeSpan>("DURATION"); }
            set
            {
                Properties.Set("DURATION", value);
                ExtrapolateTimes();
            }
        }

        virtual public IGeographicLocation GeographicLocation
        {
            get { return Properties.Get<IGeographicLocation>("GEO"); }
            set { Properties.Set("GEO", value); }
        }

        virtual public string Location
        {
            get { return Properties.Get<string>("LOCATION"); }
            set { Properties.Set("LOCATION", value); }
        }

        virtual public int PercentComplete
        {
            get { return Properties.Get<int>("PERCENT-COMPLETE"); }
            set { Properties.Set("PERCENT-COMPLETE", value); }
        }

        virtual public IList<string> Resources
        {
            get { return Properties.Get<IList<string>>("RESOURCES"); }
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
                        else Completed = default(iCalDateTime);
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
            this.Name = Components.TODO;
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

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

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
                for (int i = 0; i < Periods.Count; i++)
                {
                    Period p = Periods[i];
                    if (p.EndTime == null)
                    {
                        p.Duration = Duration;
                        if (p.Duration != null)
                            p.EndTime = p.StartTime + Duration;
                        else p.EndTime = p.StartTime;
                    }                    
                    else
                    {
                        // Ensure the Kind of time is consistent with DTStart
                        iCalDateTime dt = p.EndTime;
                        dt.IsUniversalTime = DTStart.IsUniversalTime;
                        p.EndTime = dt;
                    }

                    Periods[i] = p;
                }

                return Periods;
            }
            return new List<Period>();
        }

        #endregion

        #region Private Methods

        private void EvaluateToPreviousOccurrence(iCalDateTime completedDate, iCalDateTime currDt)
        {
            iCalDateTime beginningDate = completedDate;
            if (RecurrenceRules != null) foreach (IRecurrencePattern rrule in RecurrenceRules) DetermineStartingRecurrence(rrule, ref beginningDate);
            if (RecurrenceDates != null) foreach (IRecurrenceDate rdate in RecurrenceDates) DetermineStartingRecurrence(rdate, ref beginningDate);
            if (ExceptionRules != null) foreach (IRecurrencePattern exrule in ExceptionRules) DetermineStartingRecurrence(exrule, ref beginningDate);
            if (ExceptionDates != null) foreach (IRecurrenceDate exdate in ExceptionDates) DetermineStartingRecurrence(exdate, ref beginningDate);

            Evaluate(beginningDate, currDt);
        }

        private void DetermineStartingRecurrence(IRecurrenceDate rdate, ref iCalDateTime dt)
        {
            foreach (Period p in rdate.Periods)
            {
                if (p.StartTime < dt)
                    dt = p.StartTime;
            }
        }

        private void DetermineStartingRecurrence(IRecurrencePattern recur, ref iCalDateTime dt)
        {
            if (recur.Count != int.MinValue)
                dt = DTStart;
            else recur.IncrementDate(ref dt, -recur.Interval);
        }

        private void ExtrapolateTimes()
        {
            if (!Due.IsAssigned && DTStart.IsAssigned && Duration != default(TimeSpan))
                Due = DTStart + Duration;
            else if (Duration == default(TimeSpan) && DTStart.IsAssigned && Due.IsAssigned)
                Duration = Due - DTStart;
            else if (!DTStart.IsAssigned && Duration != default(TimeSpan) && Due.IsAssigned)
                DTStart = Due - Duration;
        }

        #endregion
    }
}
