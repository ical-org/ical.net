using System.Collections.Generic;
using ical.NET.Collections.Interfaces;

namespace ical.NET.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>,
        IMultiLinkedList<TType>
    {
        #region Private Fields

        IMultiLinkedList<TType> _previous;
        IMultiLinkedList<TType> _next;

        #endregion

        #region IMultiLinkedList<TType> Members

        public virtual void SetPrevious(IMultiLinkedList<TType> previous)
        {
            _previous = previous;
        }

        public virtual void SetNext(IMultiLinkedList<TType> next)
        {
            _next = next;
        }

        public virtual int StartIndex => _previous != null ? _previous.ExclusiveEnd : 0;

        public virtual int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;

        #endregion
    }
}
