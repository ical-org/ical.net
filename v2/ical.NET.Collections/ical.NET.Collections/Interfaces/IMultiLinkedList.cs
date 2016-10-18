using System.Collections.Generic;

namespace ical.NET.Collections.Interfaces
{
    public interface IMultiLinkedList<TType> :
        IList<TType>
    {
        int StartIndex { get; }
        int ExclusiveEnd { get; }
    }
}
