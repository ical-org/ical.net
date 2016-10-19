using System.Collections;
using System.Collections.Generic;

namespace ical.NET.Collections.Enumerators
{
    public class GroupedListEnumerator<TType> :
        IEnumerator<TType>
    {
        private readonly IList<List<TType>> _lists;
        private IEnumerator<List<TType>> _listsEnumerator;
        private IEnumerator<TType> _listEnumerator;

        public GroupedListEnumerator(IList<List<TType>> lists)
        {
            _lists = lists;
        }

        public virtual TType Current => _listEnumerator == null
            ? default(TType)
            : _listEnumerator.Current;

        public virtual void Dispose()
        {
            Reset();
        }

        private void DisposeListEnumerator()
        {
            if (_listEnumerator == null)
            {
                return;
            }

            _listEnumerator.Dispose();
            _listEnumerator = null;
        }

        object IEnumerator.Current => _listEnumerator == null
            ? default(TType)
            : _listEnumerator.Current;

        private bool MoveNextList()
        {
            if (_listsEnumerator == null)
            {
                _listsEnumerator = _lists.GetEnumerator();
            }

            if (_listsEnumerator == null)
            {
                return false;
            }

            if (!_listsEnumerator.MoveNext())
            {
                return false;
            }

            DisposeListEnumerator();
            if (_listsEnumerator.Current == null)
            {
                return false;
            }

            _listEnumerator = _listsEnumerator.Current.GetEnumerator();
            return true;
        }

        public virtual bool MoveNext()
        {
            if (_listEnumerator != null)
            {
                if (_listEnumerator.MoveNext())
                {
                    return true;
                }
                DisposeListEnumerator();
                if (MoveNextList())
                {
                    return MoveNext();
                }
            }
            else
            {
                if (MoveNextList())
                {
                    return MoveNext();
                }
            }
            return false;
        }

        public virtual void Reset()
        {
            if (_listsEnumerator == null)
            {
                return;
            }

            _listsEnumerator.Dispose();
            _listsEnumerator = null;
        }
    }
}
