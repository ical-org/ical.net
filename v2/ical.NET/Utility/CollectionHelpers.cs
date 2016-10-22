using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Utility
{
    public static class CollectionHelpers
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
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null && right != null)
            {
                return false;
            }

            if (left != null && right == null)
            {
                return false;
            }

            if (orderSignificant)
            {
                return left.SequenceEqual(right);
            }

            return left.OrderBy(l => l).SequenceEqual(right.OrderBy(r => r));
        }
    }
}
