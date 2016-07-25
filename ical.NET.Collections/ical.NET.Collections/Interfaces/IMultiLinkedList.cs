using System.Collections.Generic;

namespace ical.Net.Collections.Interfaces
{
    public interface IMultiLinkedList<TType> :
        IList<TType>
    {
        int StartIndex { get; }
        int ExclusiveEnd { get; }
    }
}
