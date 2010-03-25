using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace DDay.iCal
{
    public class CompositeList<T> :
        ICompositeList<T>
    {
        #region Public Events

        public event EventHandler<ObjectEventArgs<object>> ItemAdded;
        public event EventHandler<ObjectEventArgs<object>> ItemRemoved;

        protected void OnItemAdded(object item)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<object>(item));
        }

        protected void OnItemRemoved(object item)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<object>(item));
        }

        #endregion

        #region Private Fields

        List<IList<T>> m_Lists;
        ICalendarObject m_Parent;

        #endregion

        #region Constructors

        public CompositeList(ICalendarObject parent)
        {            
            m_Lists = new List<IList<T>>();
            AssociationUtil.AssociateList(this, parent);
        }

        public CompositeList(ICalendarObject parent, IEnumerable list) : this(parent)
        {
            AddList(list);
        }

        public CompositeList(ICalendarObject parent, IEnumerable<IEnumerable> lists) : this(parent)
        {
            AddListRange(lists);
        }

        #endregion

        #region Protected Methods

        protected IList<T> ListForIndex(int index, out int indexInList)
        {
            int count = 0;
            int listIndex = 0;

            // Search through the lists for the one
            // that contains the index in question.
            while (
                listIndex < m_Lists.Count &&
                (
                    m_Lists[listIndex].Count == 0 || // This list needs to be skipped
                    count + m_Lists[listIndex].Count <= index // The desired index is in the next list
                ))
            {
                count += m_Lists[listIndex++].Count;
            }

            if (listIndex < m_Lists.Count &&
                count <= index &&
                m_Lists[listIndex].Count > index - count)
            {
                indexInList = index - count;
                return m_Lists[listIndex];                
            }

            indexInList = -1;
            return null;
        }

        protected IList<T> ListForItem(T item, out int indexInList)
        {
            int i = 0;
            foreach (IList<T> list in m_Lists)
            {
                int index = list.IndexOf(item);
                if (index >= 0)
                {
                    indexInList = i + index;
                    return list;
                }
                else
                {
                    i += list.Count;
                }
            }
            indexInList = -1;
            return null;
        }

        protected IList<T> GetListForEnumerable(IEnumerable list)
        {
            if (list == null)
                return null;

            if (list is IList<T>)
            {
                // The list is already of the correct type, return it!
                return (IList<T>)list;
            }
            else
            {
                // Build a new list that contains only items that
                // can be added to the list.
                List<T> l = new List<T>();
                foreach (object obj in list)
                {
                    if (obj is T)
                        l.Add((T)obj);
                }
                return l;
            }
        }

        #endregion

        #region ICompositeList<T> Members

        virtual public void AddList(IEnumerable e)
        {
            IList<T> list = GetListForEnumerable(e);
            if (list != null)
            {
                m_Lists.Add(list);
                foreach (T item in list)
                    OnItemAdded(item);
            }
        }

        virtual public void RemoveList(IList<T> list)
        {
            if (list != null)
            {
                m_Lists.Remove(list);
                foreach (T item in list)
                    OnItemRemoved(item);
            }
        }

        virtual public void AddListRange(IEnumerable<IEnumerable> lists)
        {
            if (lists != null)
            {
                foreach (IEnumerable item in lists)
                    AddList(GetListForEnumerable(item));
            }
        }

        #endregion

        #region IList<T> Members

        virtual public int IndexOf(T item)
        {
            int indexInList = 0;
            ListForItem(item, out indexInList);
            return indexInList;
        }

        virtual public void Insert(int index, T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            int indexInList;
            IList<T> list = ListForIndex(index, out indexInList);
            if (list != null)
            {
                list.Insert(indexInList, item);
                OnItemAdded(item);
            }
        }

        virtual public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            int indexInList;
            IList<T> list = ListForIndex(index, out indexInList);
            if (list != null)
            {
                T item = list[indexInList];
                list.RemoveAt(indexInList);
                OnItemRemoved(item);
            }
        }

        virtual public T this[int index]
        {
            get
            {
                int indexInList;
                IList<T> list = ListForIndex(index, out indexInList);
                if (list != null)
                    return list[indexInList];
                return default(T);
            }
            set
            {
                int indexInList;
                IList<T> list = ListForIndex(index, out indexInList);
                if (list != null)
                {
                    T oldValue = list[indexInList];
                    list[indexInList] = value;
                    OnItemRemoved(oldValue);
                    OnItemAdded(value);
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        virtual public void Add(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            // FIXME: what if there are no lists to add to?
            // Do we add one for the user?  This could have
            // many pros/cons, and should be looked at later.
            Debug.Assert(m_Lists != null && m_Lists.Count > 0);
            if (m_Lists.Count > 0)
            {
                // FIXME: should we allow some customization as to 
                // which list we add items to?

                // Add the item to the last list available.
                m_Lists[m_Lists.Count - 1].Add(item);
                OnItemAdded(item);
            }
        }

        virtual public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            List<T> items = new List<T>();
            foreach (IList<T> list in m_Lists)
            {
                foreach (T item in list)
                    items.Add(item);
                list.Clear();
            }

            foreach (T item in items)
                OnItemRemoved(item);
        }

        virtual public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        virtual public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex");

            int index = 0;
            if (array.Length >= Count)
            {
                foreach (IList<T> list in m_Lists)
                {
                    list.CopyTo(array, index + arrayIndex);
                    index += list.Count;
                }
            }
        }

        virtual public int Count
        {
            get
            {
                int count = 0;
                foreach (IList<T> list in m_Lists)
                    count += list.Count;
                return count;
            }
        }

        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        virtual public bool Remove(T item)
        {
            int indexInList;
            IList<T> list = ListForItem(item, out indexInList);
            if (list != null &&
                indexInList >= 0)
            {                
                list.RemoveAt(indexInList);
                OnItemRemoved(item);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        virtual public IEnumerator<T> GetEnumerator()
        {
            return new CompositeListEnumerator(m_Lists);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CompositeListEnumerator(m_Lists);
        }

        #endregion

        private class CompositeListEnumerator :
            IEnumerator<T>
        {
            #region Private Fields

            IList<IList<T>> m_Lists;
            IEnumerator<T> m_CurrentListEnumerator;
            int m_ListIndex = -1;

            #endregion

            #region Constructors

            public CompositeListEnumerator(IList<IList<T>> lists)
            {
                m_Lists = lists;
            }

            #endregion

            #region Private Methods

            private void MoveNextList()
            {
                if (m_ListIndex + 1 < m_Lists.Count)
                    m_CurrentListEnumerator = m_Lists[++m_ListIndex].GetEnumerator();
                else
                    m_CurrentListEnumerator = null;
            }

            #endregion

            #region IEnumerator<T> Members

            public T Current
            {
                get 
                {
                    if (m_CurrentListEnumerator != null)
                        return m_CurrentListEnumerator.Current;
                    return default(T);
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {                
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return ((IEnumerator<T>)this).Current;
                }
            }

            public bool MoveNext()
            {
                if (m_CurrentListEnumerator == null)
                    MoveNextList();
                
                if (m_CurrentListEnumerator != null)
                {
                    if (!m_CurrentListEnumerator.MoveNext())
                    {
                        MoveNextList();
                        if (m_CurrentListEnumerator != null)
                            return m_CurrentListEnumerator.MoveNext();
                    }
                    else return true;
                }
                return false;
            }

            public void Reset()
            {
                m_CurrentListEnumerator = null;
                m_ListIndex = -1;
            }

            #endregion
        }
    }
}
