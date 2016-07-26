using ical.NET.Collections.Interfaces.Proxies;
using Ical.Net.General;

namespace Ical.Net.Interfaces.General.Proxies
{
    public interface ICalendarParameterCollectionProxy : ICalendarParameterCollection, IGroupedCollectionProxy<string, CalendarParameter, CalendarParameter> {}
}