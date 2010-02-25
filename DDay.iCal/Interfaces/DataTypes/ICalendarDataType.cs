using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataType :
        ICalendarProperty
    {
        /// <summary>
        /// The calendar that this object belongs to.
        /// Since data types are more loosely-connected
        /// to the calendar, it's necessary to sometimes
        /// set this property manually, rather than determine
        /// it automatically through hierarchical structure.
        /// </summary>
        IICalendar Calendar { get; set; }
    }
}
