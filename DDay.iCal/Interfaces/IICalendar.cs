using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IICalendar :
        ICalendarComponent
    {
        /// <summary>
        /// Gets/sets the component factory for this calendar.
        /// </summary>
        ICalendarComponentFactory ComponentFactory { get; set; }

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
        ICalendarTimeZone GetTimeZone(TZID tzid);
    }
}
