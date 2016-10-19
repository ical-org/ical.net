using System.Collections.Generic;

namespace ical.NET.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>
    {
        private MultiLinkedList<TType> _previous;
        private MultiLinkedList<TType> _next;

        public virtual void SetPrevious(MultiLinkedList<TType> previous)
        {
            _previous = previous;
        }

        public virtual void SetNext(MultiLinkedList<TType> next)
        {
            _next = next;
        }

        public virtual int StartIndex => _previous?.ExclusiveEnd ?? 0;

        public virtual int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;
    }
}
