using ical.net.General;

namespace ical.net.Interfaces.General
{
    public interface ICalendarPropertyListContainer : ICalendarObject
    {
        CalendarPropertyList Properties { get; }
    }
}