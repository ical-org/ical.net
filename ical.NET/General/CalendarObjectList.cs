using System;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    /// <summary>
    /// A collection of calendar objects.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarObjectList :
        GroupedList<string, ICalendarObject>,
        ICalendarObjectList<ICalendarObject>
    {
        ICalendarObject _Parent;

        public CalendarObjectList(ICalendarObject parent)
        {
            _Parent = parent;
        }
    }
}
