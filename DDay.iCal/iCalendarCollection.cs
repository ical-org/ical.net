using System;
using System.Collections.Generic;
using System.Text;

using DDay.iCal.Components;
using DDay.iCal.DataTypes;
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
#else
    [Serializable]
#endif
    public class iCalendarCollection : ICollection<iCalendar>
    {
        #region Private Fields

        private List<iCalendar> _Calendars;

        #endregion

        #region Constructors

        public iCalendarCollection()
        {
            _Calendars = new List<iCalendar>();
        }

        #endregion

        #region Public Properties

        public IEnumerable<UniqueComponent> UniqueComponents
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (UniqueComponent uc in iCal.UniqueComponents)
                        yield return uc;
            }
        }

        public IEnumerable<RecurringComponent> RecurringComponents
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (RecurringComponent rc in iCal.RecurringComponents)
                        yield return rc;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.Event"/> components in the iCalendar.
        /// </summary>
        public IEnumerable<Event> Events
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (Event evt in iCal.Events)
                        yield return evt;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.FreeBusy"/> components in the iCalendar.
        /// </summary>
        public IEnumerable<FreeBusy> FreeBusy
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (FreeBusy fb in iCal.FreeBusy)
                        yield return fb;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.Journal"/> components in the iCalendar.
        /// </summary>
        public IEnumerable<Journal> Journals
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (Journal jnl in iCal.Journals)
                        yield return jnl;
            }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.TimeZone"/> components in the iCalendar.
        /// </summary>
        public IEnumerable<iCalTimeZone> TimeZones
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (iCalTimeZone tz in iCal.TimeZones)
                        yield return tz;
            }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        public IEnumerable<Todo> Todos
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (Todo td in iCal.Todos)
                        yield return td;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the <see cref="DDay.iCal.Components.TimeZone" /> object for the specified
        /// <see cref="TZID"/> (Time Zone Identifier).
        /// </summary>
        /// <param name="tzid">A valid <see cref="TZID"/> object, or a valid <see cref="TZID"/> string.</param>
        /// <returns>A <see cref="TimeZone"/> object for the <see cref="TZID"/>.</returns>
        public iCalTimeZone GetTimeZone(TZID tzid)
        {
            foreach (iCalendar iCal in this)
            {
                iCalTimeZone tz = iCal.GetTimeZone(tzid);
                if (tz != null)
                    return tz;                
            }
            return null;
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time.
        /// <example>
        ///     For example, if you are displaying a month-view for January 2007,
        ///     you would want to evaluate recurrences for Jan. 1, 2007 to Jan. 31, 2007
        ///     to display relevant information for those dates.
        /// </example>
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>                
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");            
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time, for
        /// the type of recurring component specified.
        /// </summary>
        /// <typeparam name="T">The type of component to be evaluated for recurrences.</typeparam>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate<T>(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        public void ClearEvaluation()
        {
            foreach (iCalendar iCal in _Calendars)
                iCal.ClearEvaluation();            
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        public List<Occurrence> GetOccurrences(iCalDateTime dt)
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
        public List<Occurrence> GetOccurrences(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            return GetOccurrences<RecurringComponent>(FromDate, ToDate);
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
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime dt) where T : RecurringComponent
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
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime startTime, iCalDateTime endTime) where T : RecurringComponent
        {
            List<Occurrence> occurrences = new List<Occurrence>();
            foreach (iCalendar iCal in _Calendars)
                occurrences.AddRange(iCal.GetOccurrences<T>(startTime, endTime));

            occurrences.Sort();
            return occurrences;
        }

        #endregion

        #region Indexer

        public iCalendar this[int index]
        {
            get { return _Calendars[index]; }
            set { _Calendars[index] = value; }
        }

        #endregion

        #region ICollection<iCalendar> Members

        public void Add(iCalendar item)
        {
            if (item != null && !Contains(item))
                _Calendars.Add(item);
        }

        public void Clear()
        {
            _Calendars.Clear();
        }

        public bool Contains(iCalendar item)
        {
            return _Calendars.Contains(item);
        }

        public void CopyTo(iCalendar[] array, int arrayIndex)
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

        public bool Remove(iCalendar item)
        {
            return _Calendars.Remove(item);
        }

        #endregion

        #region IEnumerable<iCalendar> Members

        public IEnumerator<iCalendar> GetEnumerator()
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
    }
}
