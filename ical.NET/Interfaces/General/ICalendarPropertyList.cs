using ical.NET.Collections.Interfaces;
using Ical.Net.General;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarPropertyList : IGroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        ICalendarProperty this[string name] { get; }
    }
}