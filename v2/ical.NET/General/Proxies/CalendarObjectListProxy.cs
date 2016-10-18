using System.Linq;
using ical.net.collections.Interfaces;
using ical.net.collections.Proxies;
using ical.net.Interfaces.General;

namespace ical.net.General.Proxies
{
    public class CalendarObjectListProxy<TType> : GroupedCollectionProxy<string, ICalendarObject, TType>, ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list) {}

        public virtual TType this[int index] => this.Skip(index).FirstOrDefault();
    }
}