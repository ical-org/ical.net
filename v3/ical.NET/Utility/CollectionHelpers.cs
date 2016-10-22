using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Utility
{
    internal static class CollectionHelpers
    {
        internal static int GetHashCode<T>(IEnumerable<T> collection)
        {
            unchecked
            {
                return collection?.Where(e => e != null)
                    .Aggregate(397, (current, element) => current * 397 + element.GetHashCode()) ?? 0;
            }
        }

        internal static bool Equals<T>(IEnumerable<T> left, IEnumerable<T> right, bool orderSignificant = false)
        {
            if (orderSignificant)
            {
                return left.SequenceEqual(right);
            }

            var sortedLeft = left.OrderBy(l => l).ToList();
            var sortedRight = right.OrderBy(r => r).ToList();

            return sortedLeft.SequenceEqual(sortedRight);
        }
    }
}
