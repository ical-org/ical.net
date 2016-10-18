using ical.net.collections;
using ical.net.Interfaces.General;

namespace ical.net.General
{
    /// <summary>
    /// A collection of calendar objects.
    /// </summary>
    public class CalendarObjectList : GroupedList<string, ICalendarObject>, ICalendarObjectList<ICalendarObject>
    {
        public CalendarObjectList(ICalendarObject parent) {}
    }
}