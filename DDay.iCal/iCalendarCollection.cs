using System;
using System.Collections.Generic;
using System.Text;

using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace DDay.iCal
{
    /// <summary>
    /// A collection of iCalendars that is enumerable, and contains
    /// several public properties for direct access to components
    /// from each individual iCalendar.
    /// </summary>
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
        public IEnumerable<DDay.iCal.Components.TimeZone> TimeZones
        {
            get
            {
                foreach (iCalendar iCal in this)
                    foreach (DDay.iCal.Components.TimeZone tz in iCal.TimeZones)
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
        public DDay.iCal.Components.TimeZone GetTimeZone(TZID tzid)
        {
            foreach (iCalendar iCal in this)
            {
                DDay.iCal.Components.TimeZone tz = iCal.GetTimeZone(tzid);
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
        public void Evaluate(Date_Time FromDate, Date_Time ToDate)
        {
            foreach (iCalendar iCal in _Calendars)
                iCal.Evaluate(FromDate, ToDate);
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time, for
        /// the type of recurring component specified.
        /// </summary>
        /// <typeparam name="T">The type of component to be evaluated for recurrences.</typeparam>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>
        public void Evaluate<T>(Date_Time FromDate, Date_Time ToDate)
        {
            foreach (iCalendar iCal in _Calendars)
                iCal.Evaluate<T>(FromDate, ToDate);            
        }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        public void ClearEvaluation()
        {
            foreach (iCalendar iCal in _Calendars)
                iCal.ClearEvaluation();            
        }

        ///// <summary>
        ///// Returns a list of flattened recurrences for all recurring components
        ///// in the iCalendar.
        ///// </summary>
        ///// <returns>A list of flattened recurrences for all recurring components</returns>
        //public IEnumerable<RecurringComponent> FlattenRecurrences()
        //{
        //    foreach (iCalendar iCal in _Calendars)
        //        foreach (RecurringComponent rc in iCal.FlattenRecurrences())
        //            yield return rc;
        //}

        ///// <summary>
        ///// Returns a list of flattened recurrences of type T.
        ///// </summary>
        ///// <typeparam name="T">The type for which to return flattened recurrences</typeparam>
        ///// <returns>A list of flattened recurrences of type T</returns>
        //public IEnumerable<T> FlattenRecurrences<T>()
        //{
        //    foreach (iCalendar iCal in _Calendars)
        //        foreach (T t in iCal.FlattenRecurrences<T>())
        //            yield return t;
        //}

        ///// <summary>
        ///// Returns a list of flattened recurrence instances for the given date range.
        ///// </summary>
        ///// <param name="startDate">The starting date of the date range</param>
        ///// <param name="endDate">The ending date of the date range</param>
        ///// <returns>A list of flattened recurrences for the date range</returns>
        //public IEnumerable<RecurringComponent> GetRecurrencesForRange(Date_Time startDate, Date_Time endDate)
        //{
        //    foreach(iCalendar iCal in _Calendars)
        //        foreach (RecurringComponent rc in iCal.GetRecurrencesForRange(startDate, endDate))
        //            yield return rc;
        //}

        ///// <summary>
        ///// Returns a list of flattened recurrence instances of type T for the given date range.
        ///// </summary>
        ///// <param name="startDate">The starting date of the date range</param>
        ///// <param name="endDate">The ending date of the date range</param>
        ///// <returns>A list of flattened recurrences of type T for the date range</returns>
        //public IEnumerable<T> GetRecurrencesForRange<T>(Date_Time startDate, Date_Time endDate)
        //{
        //    foreach (iCalendar iCal in _Calendars)
        //        foreach (T t in iCal.GetRecurrencesForRange<T>(startDate, endDate))
        //            yield return t;
        //}

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
