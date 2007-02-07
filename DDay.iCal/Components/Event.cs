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
    /// A class that represents an RFC 2445 VEVENT component.
    /// </summary>
    /// <note>
    ///     TODO: Add support for the following properties:
    ///     <list type="bullet">
    ///         <item>Add support for the Organizer and Attendee properties</item>
    ///         <item>Add support for the Class property</item>
    ///         <item>Add support for the Geo property</item>
    ///         <item>Add support for the Priority property</item>
    ///         <item>Add support for the Related property</item>
    ///         <item>Create a TextCollection DataType for 'text' items separated by commas</item>
    ///     </list>
    /// </note>
    [DebuggerDisplay("{Summary}: {Start} {Duration}")]
    public class Event : RecurringComponent
    {
        #region Private Fields

        private Date_Time m_DTEnd;
        private Duration m_Duration;

        #endregion

        #region Public Fields
                
        /// <summary>
        /// The geographic location (lat/long) of the event.
        /// </summary>
        [Serialized]
        public Geo Geo;

        /// <summary>
        /// The location of the event.
        /// </summary>
        [Serialized]
        public Text Location;

        /// <summary>
        /// Resources that will be used during the event.
        /// <example>Conference room #2</example>
        /// <example>Projector</example>
        /// </summary>
        [Serialized]
        public TextCollection[] Resources;        

        /// <summary>
        /// The status of the event.
        /// </summary>
        [Serialized, DefaultValue("TENTATIVE\r\n")]
        public EventStatus Status;

        /// <summary>
        /// The transparency of the event.  In other words,
        /// whether or not the period of time this event
        /// occupies can contain other events (transparent),
        /// or if the time cannot be scheduled for anything
        /// else (opaque).
        /// </summary>
        [Serialized, DefaultValue("OPAQUE\r\n")]
        public Transparency Transp;        

        #endregion
         
        #region Public Properties

        /// <summary>
        /// The start date/time of the event.
        /// <note>
        /// If the duration has not been set, but
        /// the start/end time of the event is available,
        /// the duration is automatically determined.
        /// Likewise, if the end date/time has not been
        /// set, but a start and duration are available,
        /// the end date/time will be extrapolated.
        /// </note>
        /// </summary>
        [Serialized]
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
        /// The end date/time of the event.
        /// <note>
        /// If the duration has not been set, but
        /// the start/end time of the event is available,
        /// the duration is automatically determined.
        /// Likewise, if an end time and duration are available,
        /// but a start time has not been set, the start time
        /// will be extrapolated.
        /// </note>
        /// </summary>
        [Serialized, DefaultValueType("DATE-TIME")]
        virtual public Date_Time DTEnd
        {
            get { return m_DTEnd; }
            set
            {
                m_DTEnd = value;
                ExtrapolateTimes();
            }
        }

        /// <summary>
        /// The duration of the event.
        /// <note>
        /// If a start time and duration is available,
        /// the end time is automatically determined.
        /// Likewise, if the end time and duration is
        /// available, but a start time is not determined,
        /// the start time will be extrapolated from
        /// available information.
        /// </note>
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
        /// An alias to the DTEnd field (i.e. end date/time).
        /// </summary>
        virtual public Date_Time End
        {
            get { return DTEnd; }
            set { DTEnd = value; }
        }

        /// <summary>
        /// Returns true of the event is an all-day event.
        /// </summary>
        virtual public bool IsAllDay
        {
            get { return Start != null && !Start.HasTime; }
            set
            {
                // Set whether or not the start date/time
                // has a time value.
                if (Start != null)
                    Start.HasTime = !value;
            }
        }

        #endregion

        #region Static Public Methods

        static public Event Create(iCalendar iCal)
        {
            Event evt = (Event)iCal.Create(iCal, "VEVENT");
            evt.UID = UniqueComponent.NewUID();
            evt.Created = DateTime.Now;
            evt.DTStamp = DateTime.Now;

            return evt;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an Event object, with an <see cref="iCalObject"/>
        /// (usually an iCalendar object) as its parent.
        /// </summary>
        /// <param name="parent">An <see cref="iCalObject"/>, usually an iCalendar object.</param>
        public Event(iCalObject parent) : base(parent)
        {
            this.Name = "VEVENT";            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Use this method to determine if an event occurs on a given date.
        /// <note type="caution">
        ///     This event should be called only after the <see cref="Evaluate"/>
        ///     method has calculated the dates for which this event occurs.
        /// </note>
        /// </summary>
        /// <param name="DateTime">The date to test.</param>
        /// <returns>True if the event occurs on the <paramref name="DateTime"/> provided, False otherwise.</returns>
        public bool OccursOn(Date_Time DateTime)
        {            
            foreach (Period p in Periods)
                // NOTE: removed UTC from date checks, since a date is a date.
                if (p.StartTime.Date == DateTime.Date ||    // It's the start date
                    (p.StartTime.Date <= DateTime.Date &&   // It's after the start date AND
                    (p.EndTime.HasTime && p.EndTime.Date >= DateTime.Date || // an end time was specified, and it's before then
                    (!p.EndTime.HasTime && p.EndTime.Date > DateTime.Date)))) // an end time was not specified, and it's before the end date
                    // NOTE: fixed bug as follows:
                    // DTSTART;VALUE=DATE:20060704
                    // DTEND;VALUE=DATE:20060705
                    // Event.OccursOn(new Date_Time(2006, 7, 5)); // Evals to true; should be false
                    return true;
            return false;
        }

        /// <summary>
        /// Use this method to determine if an event begins at a given date and time.
        /// </summary>
        /// <param name="DateTime">The date and time to test.</param>
        /// <returns>True if the event begins at the given date and time</returns>
        public bool OccursAt(Date_Time DateTime)
        {            
            foreach (Period p in Periods)
                if (p.StartTime.Equals(DateTime))
                    return true;
            return false;
        }

        /// <summary>
        /// Determines whether or not the <see cref="Event"/> is actively displayed
        /// as an upcoming or occurred event.
        /// </summary>
        /// <returns>True if the event has not been cancelled, False otherwise.</returns>
        public bool IsActive()
        {
            return (Status != EventStatus.CANCELLED);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates this event to determine the dates and times for which the event occurs.
        /// This method only evaluates events which occur between <paramref name="FromDate"/>
        /// and <paramref name="ToDate"/>; therefore, if you require a list of events which
        /// occur outside of this range, you must specify a <paramref name="FromDate"/> and
        /// <paramref name="ToDate"/> which encapsulate the date(s) of interest.
        /// <note type="caution">
        ///     For events with very complex recurrence rules, this method may be a bottleneck
        ///     during processing time, especially when this method in called for a large number
        ///     of events, in sequence, or for a very large time span.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        /// <returns></returns>                
        public override List<Period> Evaluate(Date_Time FromDate, Date_Time ToDate)
        {
            // Add the event itself, before recurrence rules are evaluated
            // NOTE: this fixes a bug where (if evaluated multiple times)
            // a period can be added to the Periods collection multiple times.
            Period period = new Period(DTStart, Duration);
            // Ensure the period does not already exist in our collection
            if (!Periods.Contains(period))
                Periods.Add(period);

            // Evaluate recurrences normally
            base.Evaluate(FromDate, ToDate);

            // Ensure each period has a duration
            foreach(Period p in Periods)
            {
                if (p.EndTime == null)
                {
                    p.Duration = Duration;
                    p.EndTime = p.StartTime + Duration;
                }
                // Ensure the Kind of time is consistent with DTStart
                else if (p.EndTime.Kind != DTStart.Kind)
                {
                    p.EndTime.Value = DateTime.SpecifyKind(p.EndTime.Value, DTStart.Kind);;
                }
            }
                        
            return Periods;
        }
        
        /// <summary>
        /// Returns a typed copy of the Event object.
        /// </summary>
        /// <returns>A typed copy of the Event object.</returns>
        public Event Copy()
        {
            return (Event)base.Copy();
        }

        #endregion        

        #region Private Methods

        private void ExtrapolateTimes()
        {
            if (DTEnd == null && DTStart != null && Duration != null)
                DTEnd = DTStart + Duration;
            else if (Duration == null && DTStart != null && DTEnd != null)
                Duration = DTEnd - DTStart;
            else if (DTStart == null && Duration != null && DTEnd != null)
                DTStart = DTEnd - Duration;
        }

        #endregion
    }
}
