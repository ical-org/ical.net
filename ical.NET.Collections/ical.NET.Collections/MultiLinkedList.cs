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

        virtual public void SetPrevious(IMultiLinkedList<TType> previous)
        {
            _previous = previous;
        }

        virtual public void SetNext(IMultiLinkedList<TType> next)
        {
            _next = next;
        }

        virtual public int StartIndex
        {
            get { return _previous != null ? _previous.ExclusiveEnd : 0; }
        }

        virtual public int ExclusiveEnd
        {
            get { return Count > 0 ? StartIndex + Count : StartIndex; }
        }

        #endregion
    }
}
