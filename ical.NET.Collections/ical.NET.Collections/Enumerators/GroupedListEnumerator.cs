using System.Collections.Generic;
using ical.NET.Collections.Interfaces;

namespace ical.NET.Collections.Enumerators
{
    public class GroupedListEnumerator<TType> :
        IEnumerator<TType>
    {
        IList<IMultiLinkedList<TType>> _lists;
        IEnumerator<IMultiLinkedList<TType>> _listsEnumerator;
        IEnumerator<TType> _listEnumerator;

        public GroupedListEnumerator(IList<IMultiLinkedList<TType>> lists)
        {
            _lists = lists;
        }

        virtual public TType Current
        {
            get
            {
                if (_listEnumerator != null)
                    return _listEnumerator.Current;
                return default(TType);
            }
        }

        virtual public void Dispose()
        {
            Reset();
        }

        void DisposeListEnumerator()
        {
            if (_listEnumerator != null)
            {
                _listEnumerator.Dispose();
                _listEnumerator = null;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                if (_listEnumerator != null)
                    return _listEnumerator.Current;
                return default(TType);
            }
        }

        private bool MoveNextList()
        {
            if (_listsEnumerator == null)
            {
                _listsEnumerator = _lists.GetEnumerator();
            }

            if (_listsEnumerator != null)
            {
                if (_listsEnumerator.MoveNext())
                {
                    DisposeListEnumerator();
                    if (_listsEnumerator.Current != null)
                    {
                        _listEnumerator = _listsEnumerator.Current.GetEnumerator();
                        return true;
                    }
                }
            }

            return false;
        }

        virtual public bool MoveNext()
        {
            if (_listEnumerator != null)
            {
                if (_listEnumerator.MoveNext())
                {
                    return true;
                }
                else
                {
                    DisposeListEnumerator();
                    if (MoveNextList())
                        return MoveNext();
                }
            }
            else
            {
                if (MoveNextList())
                    return MoveNext();
            }
            return false;
        }

        virtual public void Reset()
        {

            if (_listsEnumerator != null)
            {
                _listsEnumerator.Dispose();
                _listsEnumerator = null;
            }
        }
    }
}
