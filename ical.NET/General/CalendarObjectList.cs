using System;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    /// <summary>
    /// A collection of calendar objects.
    /// </summary>

    [Serializable]

    public class CalendarObjectList :
        GroupedList<string, ICalendarObject>,
        ICalendarObjectList<ICalendarObject>
    {
        ICalendarObject _parent;

        public CalendarObjectList(ICalendarObject parent)
        {
            _parent = parent;
        }
    }
}
