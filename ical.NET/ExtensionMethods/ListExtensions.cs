using System.Collections.Generic;

namespace DDay.iCal
{
    public static class ListExtensions
    {
        static public void AddRange<T>(this IList<T> list, IEnumerable<T> values)
        {
            if (values != null)
            {
                foreach (var item in values)
                    list.Add(item);
            }
        }
    }
}
