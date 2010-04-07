using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VEVENT component.
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
#if DATACONTRACT
    [DataContract(Name = "Event", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
    //[KnownType(typeof(Alarm))]
#endif
    [Serializable]    
    public class Event : 
        RecurringComponent,
        IEvent
    {
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
        public override IDateTime DTStart
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
        virtual public IDateTime DTEnd
        {
            get { return Properties.Get<IDateTime>("DTEND"); }
            set
            {
                if (!object.Equals(DTEnd, value))
                {
                    Properties.Set("DTEND", value);
                    ExtrapolateTimes();
                }
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
        // NOTE: Duration is not supported by all systems,
        // (i.e. iPhone) and cannot co-exist with DTEnd.
        // RFC 5545 states:
        //
        //      ; either 'dtend' or 'duration' may appear in
        //      ; a 'eventprop', but 'dtend' and 'duration'
        //      ; MUST NOT occur in the same 'eventprop'
        //
        // Therefore, Duration is not serialized, as DTEnd
        // should always be extrapolated from the duration.
        virtual public TimeSpan Duration
        {
            get { return Properties.Get<TimeSpan>("DURATION"); }
            set
            {
                if (!object.Equals(Duration, value))
                {
                    Properties.Set("DURATION", value);
                    ExtrapolateTimes();
                }
            }
        }

        /// <summary>
        /// An alias to the DTEnd field (i.e. end date/time).
        /// </summary>
        virtual public IDateTime End
        {
            get { return DTEnd; }
            set { DTEnd = value; }
        }

        /// <summary>
        /// Returns true if the event is an all-day event.
        /// </summary>
        virtual public bool IsAllDay
        {
            get { return !Start.HasTime; }
            set
            {
                // Set whether or not the start date/time
                // has a time value.
                if (Start != null)
                    Start.HasTime = !value;
                if (End != null)
                    End.HasTime = !value;

                if (value && 
                    Start != null &&
                    End != null &&
                    object.Equals(Start.Date, End.Date))
                {
                    Duration = default(TimeSpan);
                    End = Start.AddDays(1);
                }
            }
        }

        /// <summary>
        /// The geographic location (lat/long) of the event.
        /// </summary>
        public IGeographicLocation GeographicLocation
        {
            get { return Properties.Get<IGeographicLocation>("GEO"); }
            set { Properties.Set("GEO", value); }
        }

        /// <summary>
        /// The location of the event.
        /// </summary>
        public string Location
        {
            get { return Properties.Get<string>("LOCATION"); }
            set { Properties.Set("LOCATION", value); }
        }

        /// <summary>
        /// Resources that will be used during the event.
        /// <example>Conference room #2</example>
        /// <example>Projector</example>
        /// </summary>
        public IList<string> Resources
        {
            get { return Properties.GetList<string>("RESOURCES"); }
            set { Properties.SetList("RESOURCES", value); }
        }

        /// <summary>
        /// The status of the event.
        /// </summary>
        public EventStatus Status
        {
            get { return Properties.Get<EventStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        }

        /// <summary>
        /// The transparency of the event.  In other words,
        /// whether or not the period of time this event
        /// occupies can contain other events (transparent),
        /// or if the time cannot be scheduled for anything
        /// else (opaque).
        /// </summary>
        public ITransparency Transparency
        {
            get { return Properties.Get<ITransparency>("TRANSP"); }
            set { Properties.Set("TRANSP", value); }
        }

        #endregion

        #region Private Fields

        EventEvaluator m_Evaluator;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an Event object, with an <see cref="iCalObject"/>
        /// (usually an iCalendar object) as its parent.
        /// </summary>
        /// <param name="parent">An <see cref="iCalObject"/>, usually an iCalendar object.</param>
        public Event() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Name = Components.EVENT;

            m_Evaluator = new EventEvaluator(this);
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
        virtual public bool OccursOn(IDateTime DateTime)
        {
            foreach (IPeriod p in m_Evaluator.Periods)
                // NOTE: removed UTC from date checks, since a date is a date.
                if (p.StartTime.Date == DateTime.Date ||    // It's the start date OR
                    (p.StartTime.Date <= DateTime.Date &&   // It's after the start date AND
                    (p.EndTime.HasTime && p.EndTime.Date >= DateTime.Date || // an end time was specified, and it's after the test date
                    (!p.EndTime.HasTime && p.EndTime.Date > DateTime.Date)))) // an end time was not specified, and it's before the end date
                    // NOTE: fixed bug as follows:
                    // DTSTART;VALUE=DATE:20060704
                    // DTEND;VALUE=DATE:20060705
                    // Event.OccursOn(new iCalDateTime(2006, 7, 5)); // Evals to true; should be false
                    return true;
            return false;
        }

        /// <summary>
        /// Use this method to determine if an event begins at a given date and time.
        /// </summary>
        /// <param name="DateTime">The date and time to test.</param>
        /// <returns>True if the event begins at the given date and time</returns>
        virtual public bool OccursAt(IDateTime DateTime)
        {
            foreach (IPeriod p in m_Evaluator.Periods)
                if (p.StartTime.Equals(DateTime))
                    return true;
            return false;
        }

        /// <summary>
        /// Determines whether or not the <see cref="Event"/> is actively displayed
        /// as an upcoming or occurred event.
        /// </summary>
        /// <returns>True if the event has not been cancelled, False otherwise.</returns>
        virtual public bool IsActive()
        {
            return (Status != EventStatus.Cancelled);            
        }

        #endregion

        #region Overrides

        protected override bool EvaluationIncludesReferenceDate
        {
            get
            {
                return true;
            }
        }

        public override object GetService(Type serviceType)
        {
            if (typeof(IEvaluator).IsAssignableFrom(serviceType))
                return m_Evaluator;
            return null;
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            ExtrapolateTimes();
        }        
        
        #endregion

        #region Private Methods

        private void ExtrapolateTimes()
        {
            if (DTEnd == null && DTStart != null && Duration != default(TimeSpan))
                DTEnd = DTStart.Add(Duration);
            else if (Duration == default(TimeSpan) && DTStart != null && DTEnd != null)
                Duration = DTEnd.Subtract(DTStart);
            else if (DTStart == null && Duration != default(TimeSpan) && DTEnd != null)
                DTStart = DTEnd.Subtract(Duration);
        }

        #endregion
    }
}
