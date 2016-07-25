using ical.Net.Collections.Interfaces.Proxies;

namespace Ical.Net.Interfaces.General.Proxies
{
    public interface ICalendarParameterCollectionProxy : ICalendarParameterCollection, IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter> {}
}