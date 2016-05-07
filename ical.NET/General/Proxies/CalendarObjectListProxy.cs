using System.Linq;
using ical.NET.Collections.Interfaces;
using ical.NET.Collections.Proxies;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General.Proxies
{
    public class CalendarObjectListProxy<TType> :
        GroupedCollectionProxy<string, ICalendarObject, TType>,
        ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list)
        {
        }

        virtual public TType this[int index]
        {
            get
            {
                return this.Skip(index).FirstOrDefault();
            }
        }
    }
}
