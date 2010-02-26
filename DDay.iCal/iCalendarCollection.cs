using System;
using System.Collections.Generic;
using System.Text;

using DDay.iCal;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A collection of iCalendars that is enumerable, and contains
    /// several public properties for direct access to components
    /// from each individual iCalendar.
    /// </summary>
#if DATACONTRACT
    [CollectionDataContract(Name = "iCalendarCollection", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class iCalendarCollection : 
        CalendarComponent,
        ICollection<IICalendar>,
        IICalendar
    {
        #region Private Fields

        private IList<IICalendar> _Calendars;

        #endregion

        #region Constructors

        public iCalendarCollection()
        {
            _Calendars = new List<IICalendar>();
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        virtual public void ClearEvaluation()
        {
            foreach (IICalendar iCal in _Calendars)
                iCal.ClearEvaluation();            
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        virtual public List<Occurrence> GetOccurrences(iCalDateTime dt)
        {
            return GetOccurrences(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range.</param>
        /// <param name="ToDate">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        virtual public List<Occurrence> GetOccurrences(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            return GetOccurrences<IRecurringComponent>(FromDate, ToDate);
        }

        /// <summary>
        /// Returns all occurrences of components of type T that start on the date provided.
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
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime dt) where T : IRecurringComponent
        {
            return GetOccurrences<T>(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime startTime, iCalDateTime endTime) where T : IRecurringComponent
        {
            List<Occurrence> occurrences = new List<Occurrence>();
            foreach (IICalendar iCal in _Calendars)
                occurrences.AddRange(iCal.GetOccurrences<T>(startTime, endTime));

            occurrences.Sort();
            return occurrences;
        }

        #endregion

        #region Indexer

        public IICalendar this[int index]
        {
            get { return _Calendars[index]; }
            set { _Calendars[index] = value; }
        }

        #endregion

        #region ICollection<IICalendar> Members

        public void Add(IICalendar item)
        {
            if (item != null && !Contains(item))
                _Calendars.Add(item);
        }

        public void Clear()
        {
            _Calendars.Clear();
        }

        public bool Contains(IICalendar item)
        {
            return _Calendars.Contains(item);
        }

        public void CopyTo(IICalendar[] array, int arrayIndex)
        {
            _Calendars.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _Calendars.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IICalendar item)
        {
            return _Calendars.Remove(item);
        }

        #endregion

        #region IEnumerable<IICalendar> Members

        public IEnumerator<IICalendar> GetEnumerator()
        {
            return _Calendars.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Calendars.GetEnumerator();
        }

        #endregion        

        #region IICalendar Members

        virtual public string Version
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].Version;
                return null;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.Version = value;
            }
        }

        virtual public string ProductID
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].ProductID;
                return null;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.ProductID = value;
            }
        }

        virtual public string Scale
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].Scale;
                return null;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.Scale = value;
            }
        }

        virtual public string Method
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].Method;
                return null;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.Method = value;
            }
        }

        /// <summary>
        /// A calendar collection cannot create components directly!
        /// </summary>
        virtual public ICalendarComponentFactory ComponentFactory
        {
            get { return null; }
            set { }
        }

        virtual public RecurrenceRestrictionType RecurrenceRestriction
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].RecurrenceRestriction;
                return RecurrenceRestrictionType.Default;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.RecurrenceRestriction = value;
            }
        }

        virtual public RecurrenceEvaluationModeType RecurrenceEvaluationMode
        {
            get
            {
                if (_Calendars.Count > 0)
                    return _Calendars[0].RecurrenceEvaluationMode;
                return RecurrenceEvaluationModeType.Default;
            }
            set
            {
                foreach (IICalendar c in _Calendars)
                    c.RecurrenceEvaluationMode = value;
            }
        }

        virtual public T Create<T>() where T : ICalendarComponent
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the <see cref="DDay.iCal.TimeZone" /> object for the specified
        /// <see cref="TZID"/> (Time Zone Identifier).
        /// </summary>
        /// <param name="tzid">A valid <see cref="TZID"/> object, or a valid <see cref="TZID"/> string.</param>
        /// <returns>A <see cref="TimeZone"/> object for the <see cref="TZID"/>.</returns>
        virtual public ICalendarTimeZone GetTimeZone(TZID tzid)
        {
            foreach (IICalendar c in _Calendars)
            {
                ICalendarTimeZone tz = c.GetTimeZone(tzid);
                if (tz != null)
                    return tz;
            }
            return null;
        }

        /// <summary>
        /// A collection of unique components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<IUniqueComponent> UniqueComponents
        {
            get
            {
                UniqueComponentList<IUniqueComponent> unique = new UniqueComponentList<IUniqueComponent>();

                foreach (IICalendar iCal in this)
                    foreach (IUniqueComponent uc in iCal.UniqueComponents)
                        unique.Add(uc);

                return unique;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Event"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<Event> Events
        {
            get
            {
                UniqueComponentList<Event> events = new UniqueComponentList<Event>();

                foreach (IICalendar iCal in this)
                    foreach (Event evt in iCal.Events)
                        events.Add(evt);

                return events;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.FreeBusy"/> components in the iCalendar.
        /// </summary>
        public IUniqueComponentListReadonly<FreeBusy> FreeBusy
        {
            get
            {
                UniqueComponentList<FreeBusy> freebusy = new UniqueComponentList<FreeBusy>();

                foreach (IICalendar iCal in this)
                    foreach (FreeBusy fb in iCal.FreeBusy)
                        freebusy.Add(fb);

                return freebusy;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Journal"/> components in the iCalendar.
        /// </summary>
        public IUniqueComponentListReadonly<Journal> Journals
        {
            get
            {
                UniqueComponentList<Journal> journals = new UniqueComponentList<Journal>();

                foreach (IICalendar iCal in this)
                    foreach (Journal jnl in iCal.Journals)
                        journals.Add(jnl);

                return journals;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.TimeZone"/> components in the iCalendar.
        /// </summary>
        public IList<ICalendarTimeZone> TimeZones
        {
            get
            {
                List<ICalendarTimeZone> timeZones = new List<ICalendarTimeZone>();
                foreach (IICalendar iCal in this)
                    foreach (ICalendarTimeZone tz in iCal.TimeZones)
                        timeZones.Add(tz);
                return timeZones;
            }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        public IUniqueComponentListReadonly<Todo> Todos
        {
            get
            {
                UniqueComponentList<Todo> todos = new UniqueComponentList<Todo>();

                foreach (IICalendar iCal in this)
                    foreach (Todo td in iCal.Todos)
                        todos.Add(td);

                return todos;
            }
        }

#if DATACONTRACT && !SILVERLIGHT
        /// <summary>
        /// Adds a system time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>
        /// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
        /// <returns>The time zone added to the calendar.</returns>
        public ICalendarTimeZone AddTimeZone(System.TimeZoneInfo tzi)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the local system time zone to the iCalendar.  
        /// This time zone may then be used in date/time
        /// objects contained in the calendar.
        /// </summary>
        /// <returns>The time zone added to the calendar.</returns>
        public ICalendarTimeZone AddLocalTimeZone()
        {
            throw new NotImplementedException();
        }
#endif

        #endregion
    }
}
