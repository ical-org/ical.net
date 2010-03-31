using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public class CalendarPropertyCompositeList<T> :
        ICalendarPropertyCompositeList<T>
    {
        #region Private Fields

        ICalendarObject m_Parent;
        ICalendarPropertyList m_PropertyList;
        string m_PropertyName;

        #endregion

        #region Constructors

        public CalendarPropertyCompositeList(ICalendarPropertyList propertyList, string propertyName)
        {
            m_PropertyList = propertyList;
            m_PropertyName = propertyName;

            propertyList.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarProperty>>(properties_ItemAdded);
            propertyList.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarProperty>>(properties_ItemRemoved);
        }        

        #endregion

        #region Protected Methods

        protected int PropertyValueCount(object value, out bool isList)
        {
            isList = false;
            if (value is IList<object>)
            {
                isList = true;

                int count = 0;
                foreach (object obj in (IList<object>)value)
                {
                    if (obj is T)
                        count++;
                }
                return count;
            }
            else if (value != null)
            {
                return 1;
            }
            return 0;
        }

        protected ICalendarProperty PropertyForIndex(int index, out bool isList, out int propertyIndex, out int indexInProperty)
        {
            int count = 0;
            propertyIndex = 0;
            isList = false;

            // FIXME: this method is messed up I think.

            // Search through the properties for the one
            // that contains the index in question.
            int propertyValueCount = 0;
            while (propertyIndex < m_PropertyList.Count)                
            {
                if (string.Equals(m_PropertyList[propertyIndex].Name, m_PropertyName))
                {
                    propertyValueCount = PropertyValueCount(m_PropertyList[propertyIndex].Value, out isList);
                    if (propertyValueCount > 0) // The value contains items
                    {
                        if (count + propertyValueCount > index) // The desired index is in this property
                            break;

                        count += propertyValueCount;
                    }
                }
                propertyIndex++;
            }

            if (propertyIndex < m_PropertyList.Count &&
                count <= index &&
                propertyValueCount > index - count)
            {
                indexInProperty = index - count;
                return m_PropertyList[propertyIndex];
            }

            propertyIndex = -1;
            indexInProperty = -1;
            return null;
        }

        protected ICalendarProperty PropertyForItem(T item, out bool isList, out int itemIndex, out int indexInProperty)
        {            
            itemIndex = 0;
            foreach (ICalendarProperty p in m_PropertyList)
            {
                if (p.Name.Equals(m_PropertyName))
                {
                    if (p.Value is IList<object>)
                    {
                        IList<object> list = (IList<object>)p.Value;
                        indexInProperty = list.IndexOf(item);
                        if (indexInProperty >= 0)
                        {
                            isList = true;
                            itemIndex += indexInProperty;
                            return p;
                        }
                        else itemIndex += list.Count;
                    }
                    else
                    {
                        if (object.Equals(item, p.Value))
                        {
                            isList = false;
                            indexInProperty = -1;
                            return p;
                        }
                        else
                        {
                            itemIndex++;
                        }
                    }
                }
            }
            isList = false;
            indexInProperty = -1;
            itemIndex = -1;
            return null;
        }

        #endregion

        #region Event Handlers

        void properties_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
        }

        void properties_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            bool isList;
            int itemIndex, indexInProperty;
            ICalendarProperty p = PropertyForItem(item, out isList, out itemIndex, out indexInProperty);
            return itemIndex;
        }

        public void Insert(int index, T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            bool isList;
            int propertyIndex, indexInProperty;
            ICalendarProperty p = PropertyForIndex(index, out isList, out propertyIndex, out indexInProperty);
            if (p != null)
            {
                if (isList)
                    ((IList<object>)p.Value).Insert(indexInProperty, item);
                else
                {
                    ICalendarProperty newProperty = p.Copy<ICalendarProperty>();
                    newProperty.Value = item;
                    m_PropertyList.Insert(propertyIndex, newProperty);
                }
            }
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            bool isList;
            int propertyIndex, indexInProperty;
            ICalendarProperty p = PropertyForIndex(index, out isList, out propertyIndex, out indexInProperty);
            if (p != null)
            {
                if (isList)
                    ((IList<object>)p.Value).RemoveAt(indexInProperty);
                else
                    m_PropertyList.RemoveAt(propertyIndex);
            }
        }

        public T this[int index]
        {
            get
            {
                bool isList;
                int propertyIndex, indexInProperty;
                ICalendarProperty p = PropertyForIndex(index, out isList, out propertyIndex, out indexInProperty);
                if (p != null)
                {
                    object value;
                    if (isList)
                        value = ((IList<object>)p.Value)[indexInProperty];
                    else
                        value = p.Value;

                    if (value is T)
                        return (T)value;
                }
                return default(T);
            }
            set
            {
                bool isList;
                int propertyIndex, indexInProperty;
                ICalendarProperty p = PropertyForIndex(index, out isList, out propertyIndex, out indexInProperty);
                if (p != null)
                {
                    object oldValue;
                    if (isList)
                    {
                        oldValue = ((IList<object>)p)[indexInProperty];
                        ((IList<object>)p)[indexInProperty] = value;
                    }
                    else
                    {
                        oldValue = p.Value;
                        p.Value = value;
                    }
                    
                    T old = oldValue is T ? (T)oldValue : default(T);
                    // FIXME: Call OnItemAdded()/OnItemRemoved() here?
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            
            // FIXME: Do we always add a property, or should we aggregate to
            // another property that is an IList<object>?
            CalendarProperty p = new CalendarProperty();
            p.Name = m_PropertyName;
            p.Value = item;

            m_PropertyList.Add(p);
        }

        public void Clear()
        {
            for (int i = m_PropertyList.Count - 1; i >= 0; i--)
            {
                if (string.Equals(m_PropertyList[i].Name, m_PropertyName))
                    m_PropertyList.RemoveAt(i);
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
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
                foreach (ICalendarProperty p in m_PropertyList)
                {
                    if (string.Equals(p.Name, m_PropertyName))
                    {
                        if (p.Value is IList<object>)
                        {
                            IList<object> list = (IList<object>)p.Value;
                            for (int i = 0; i < list.Count; i++)
                                array.SetValue(list[i], i + index + arrayIndex);
                            index += list.Count;
                        }
                        else if (p.Value is T)
                        {
                            array[index + arrayIndex] = (T)p.Value;
                            index++;
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (ICalendarProperty p in m_PropertyList)
                {
                    if (string.Equals(p.Name, m_PropertyName))
                    {
                        if (p.Value is IList<object>)
                        {
                            foreach (object obj in (IList<object>)p.Value)
                            {
                                if (obj is T)
                                    count++;
                            }
                        }
                        else if (p.Value != null)
                            count++;
                    }
                }
                return count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            bool isList;
            int itemIndex, indexInProperty;
            ICalendarProperty p = PropertyForItem(item, out isList, out itemIndex, out indexInProperty);
            if (p != null)
            {
                if (isList)
                {
                    ((IList<object>)p.Value).RemoveAt(indexInProperty);
                    return true;
                }
                else
                {
                    return m_PropertyList.Remove(p);
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new CalendarPropertyCompositeListEnumerator<T>(m_PropertyList, m_PropertyName);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CalendarPropertyCompositeListEnumerator<T>(m_PropertyList, m_PropertyName);
        }

        #endregion

        private class SingleValueEnumerator<T> :
            IEnumerator<T>
        {
            private bool m_IsMoved;
            private T m_Current;
            private T m_Item;

            #region Constructors

            public SingleValueEnumerator(T item)
            {
                m_Item = item;
            }

            #endregion

            #region IEnumerator<T> Members

            public T Current
            {
                get { return m_Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {                
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
                    return m_IsMoved ? (object)m_Current : null;
                }
            }

            public bool MoveNext()
            {
                if (!m_IsMoved)
                {
                    m_Current = m_Item;
                    m_IsMoved = true;
                    return true;
                }
                else
                {
                    m_Current = default(T);
                }
                return false;
            }

            public void Reset()
            {
                m_IsMoved = false;
                m_Current = default(T);
            }

            #endregion
        }

        private class CalendarPropertyCompositeListEnumerator<T> :
            IEnumerator<T>
        {
            #region Private Fields

            string m_PropertyName;
            ICalendarPropertyList m_PropertyList;            
            IEnumerator m_CurrentListEnumerator;
            int m_PropertyIndex = -1;

            #endregion

            #region Constructors

            public CalendarPropertyCompositeListEnumerator(ICalendarPropertyList propertyList, string propertyName)
            {
                m_PropertyList = propertyList;
                m_PropertyName = propertyName;
            }

            #endregion

            #region Private Methods

            private void MoveNextProperty()
            {
                m_CurrentListEnumerator = null;
                while (m_PropertyIndex + 1 < m_PropertyList.Count)
                {
                    ICalendarProperty p = m_PropertyList[++m_PropertyIndex];
                    if (string.Equals(p.Name, m_PropertyName))
                    {
                        object value = p.Value;
                        if (value is IList<object>)
                        {
                            IList<T> list = new List<T>();
                            foreach (object obj in (IList<object>)value)
                            {
                                if (obj is T)
                                    list.Add((T)obj);
                            }
                            m_CurrentListEnumerator = list.GetEnumerator();
                            return;
                        }
                        else if (value is T)
                        {
                            m_CurrentListEnumerator = new SingleValueEnumerator<T>((T)value);
                            return;
                        }
                    }
                }
            }

            #endregion

            #region IEnumerator<T> Members

            public T Current
            {
                get
                {
                    if (m_CurrentListEnumerator != null)
                        return ((IEnumerator<T>)m_CurrentListEnumerator).Current;
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
                    MoveNextProperty();

                if (m_CurrentListEnumerator != null)
                {
                    if (!m_CurrentListEnumerator.MoveNext())
                    {
                        MoveNextProperty();
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
                m_PropertyIndex = -1;
            }

            #endregion
        }
    }
}
