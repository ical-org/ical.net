using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IICalendar :
        ICalendarComponent
    {
        /// <summary>
        /// Gets/sets the calendar version.  Defaults to "2.0".
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Gets/sets the product ID for the calendar.
        /// </summary>
        string ProductID { get; set; }

        /// <summary>
        /// Gets/sets the scale of the calendar.
        /// </summary>
        string Scale { get; set; }

        /// <summary>
        /// Gets/sets the calendar method.
        /// </summary>
        string Method { get; set; }

        ///// <summary>
        ///// Gets/sets the component factory for this calendar.
        ///// </summary>
        //ICalendarComponentFactory ComponentFactory { get; set; }
        
        /// <summary>
        /// Gets/sets the restriction on how evaluation of 
        /// recurrence patterns occurs within this calendar.
        /// </summary>
        RecurrenceRestrictionType RecurrenceRestriction { get; set; }

        /// <summary>
        /// Gets/sets the evaluation mode during recurrence
        /// evaluation.  Default is ThrowException.
        /// </summary>
        RecurrenceEvaluationModeType RecurrenceEvaluationMode { get; set; }

        /// <summary>
        /// Creates a new component, and adds it
        /// to the calendar.
        /// </summary>
        T Create<T>() where T : ICalendarComponent;

        /// <summary>
        /// Returns the time zone object that corresponds
        /// to the provided TZID, or null of no matching
        /// time zone could be found.
        /// </summary>
        ITimeZone GetTimeZone(ITZID tzid);

        /// <summary>
        /// Gets a list of unique components contained in the calendar.
        /// </summary>
        IUniqueComponentListReadonly<IUniqueComponent> UniqueComponents { get; }

        /// <summary>
        /// Gets a list of Events contained in the calendar.
        /// </summary>
        IUniqueComponentListReadonly<IEvent> Events { get; }

        /// <summary>
        /// Gets a list of Free/Busy components contained in the calendar.
        /// </summary>
        IUniqueComponentListReadonly<IFreeBusy> FreeBusy { get; }

        /// <summary>
        /// Gets a list of Journal entries contained in the calendar.
        /// </summary>
        IUniqueComponentListReadonly<IJournal> Journals { get; }

        /// <summary>
        /// Gets a list of time zones contained in the calendar.
        /// </summary>
        IList<ITimeZone> TimeZones { get; }

        /// <summary>
        /// Gets a list of To-do items contained in the calendar.
        /// </summary>
        IUniqueComponentListReadonly<ITodo> Todos { get; }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        void ClearEvaluation();

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        IList<IOccurrence> GetOccurrences(iCalDateTime dt);

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range.</param>
        /// <param name="ToDate">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        IList<IOccurrence> GetOccurrences(iCalDateTime FromDate, iCalDateTime ToDate);

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
        IList<IOccurrence> GetOccurrences<T>(iCalDateTime dt) where T : IRecurringComponent;

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        IList<IOccurrence> GetOccurrences<T>(iCalDateTime startTime, iCalDateTime endTime) where T : IRecurringComponent;

        // FIXME: add this back in:
//#if DATACONTRACT && !SILVERLIGHT
//        /// <summary>
//        /// Adds a system time zone to the iCalendar.  This time zone may
//        /// then be used in date/time objects contained in the 
//        /// calendar.
//        /// </summary>
//        /// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
//        /// <returns>The time zone added to the calendar.</returns>
//        ITimeZone AddTimeZone(System.TimeZoneInfo tzi);        

//        /// <summary>
//        /// Adds the local system time zone to the iCalendar.  
//        /// This time zone may then be used in date/time
//        /// objects contained in the calendar.
//        /// </summary>
//        /// <returns>The time zone added to the calendar.</returns>
//        ITimeZone AddLocalTimeZone();
//#endif
    }
}
