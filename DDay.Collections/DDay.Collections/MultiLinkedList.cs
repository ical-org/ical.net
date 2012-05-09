using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>,
        IMultiLinkedList<TType>
    {
        #region Private Fields

        IMultiLinkedList<TType> _Previous;
        IMultiLinkedList<TType> _Next;

        #endregion

        #region IMultiLinkedList<TType> Members

        virtual public void SetPrevious(IMultiLinkedList<TType> previous)
        {
            _Previous = previous;
        }

        virtual public void SetNext(IMultiLinkedList<TType> next)
        {
            _Next = next;
        }

        virtual public int StartIndex
        {
            get { return _Previous != null ? _Previous.ExclusiveEnd : 0; }
        }

        virtual public int ExclusiveEnd
        {
            get { return Count > 0 ? StartIndex + Count : StartIndex; }
        }

        #endregion
    }
}
