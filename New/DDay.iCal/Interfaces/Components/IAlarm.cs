using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IAlarm :
        ICalendarComponent
    {
        AlarmAction Action { get; set; }
        IAttachment Attach { get; set; }
        IAttendee[] Attendee { get; set; }
        string Description { get; set; }
        TimeSpan Duration { get; set; }
        int Repeat { get; set; }
        string Summary { get; set; }
        ITrigger Trigger { get; set; }

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, iCalDateTime FromDate, iCalDateTime ToDate);

        /// <summary>
        /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="Start"/> date/time.  If <paramref name="Start"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="Start">The earliest date/time to poll trigerred alarms for.</param>
        /// <param name="End">The latest date/time to poll trigerred alarms for.</param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        IList<AlarmOccurrence> Poll(iCalDateTime Start, iCalDateTime End);
    }    
}
