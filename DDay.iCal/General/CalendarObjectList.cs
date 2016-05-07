using System;
using DDay.Collections;

namespace DDay.iCal
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
