using Ical.Net.CalendarComponents;
using System.Collections.Generic;

namespace Ical.Net.Proxies
{
    public interface IUniqueComponentList<TComponentType> :
        ICalendarObjectList<TComponentType> where TComponentType : class, IUniqueComponent
    {
        TComponentType this[string uid] { get; set; }
        void AddRange(IEnumerable<TComponentType> collection);
    }
}