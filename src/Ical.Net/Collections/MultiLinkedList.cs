using System.Collections.Generic;

namespace Ical.Net.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>,
        IMultiLinkedList<TType>
    {
        IMultiLinkedList<TType> _previous;
        IMultiLinkedList<TType> _next;

        public void SetPrevious(IMultiLinkedList<TType> previous)
        {
            _previous = previous;
        }

        public void SetNext(IMultiLinkedList<TType> next)
        {
            _next = next;
        }

        public int StartIndex => _previous?.ExclusiveEnd ?? 0;

        public int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;
    }
}
