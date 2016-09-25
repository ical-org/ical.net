using System.Collections.Generic;
using System.Linq;

namespace ical.net.Utility
{
    public static class CollectionHelpers
    {
        public static int GetHashCode<T>(IEnumerable<T> collection)
        {
            unchecked
            {
                return collection?.Where(e => e != null)
                    .Aggregate(397, (current, element) => current * 397 + element.GetHashCode()) ?? 0;
            }
        }
    }
}
