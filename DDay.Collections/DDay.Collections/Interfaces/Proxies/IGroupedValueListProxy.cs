using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.Collections
{
    public interface IGroupedValueListProxy<TItem, TValue> :
        IList<TValue>
    {
        IEnumerable<TItem> Items { get; }
    }
}
