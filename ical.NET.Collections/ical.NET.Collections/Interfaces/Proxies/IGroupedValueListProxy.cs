using System.Collections.Generic;

namespace ical.NET.Collections.Interfaces.Proxies
{
    public interface IGroupedValueListProxy<TItem, TValue> :
        IList<TValue>
    {
        IEnumerable<TItem> Items { get; }
    }
}
