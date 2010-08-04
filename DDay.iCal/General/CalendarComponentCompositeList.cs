using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// This class works similar to the CalendarPropertyCompositeList class,
    /// but works with components instead of properties.
    /// 
    /// It consolidates components of a given name into a list,
    /// and allows you to work with directly against the
    /// components themselves.  This preserves the notion that our components
    /// are still stored directly within the calendar object, but gives
    /// us the flexibility to work with multiple components through a
    /// single (composite) list.
    /// </summary>
    public class CalendarComponentCompositeList<T> :
        KeyedList<ICalendarComponent, string>,
        IList<T>
        where T : ICalendarComponent
    {
        #region Private Fields

        ICalendarComponent m_Component;
        string m_ComponentName;

        #endregion

        #region Constructors

        public CalendarComponentCompositeList(ICalendarComponent component, string componentName)
        {
            m_Component = component;
            m_ComponentName = componentName;
        }

        #endregion

        #region IList<ICalendarComponent> Members

        public int IndexOf(T value)
        {
            int index = 0;
            for (int i = 0; i < m_Component.Children.Count; i++)
            {
                if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                {
                    index++;
                    if (object.Equals(m_ComponentName[i], value))
                        return index;
                }
            }
            return -1;
        }

        public void Insert(int index, T value)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            if (index == 0)
                m_Component.Children.Insert(0, value);
            else
            {                
                int curr = 0;
                for (int i = 0; i < m_Component.Children.Count; i++)
                {
                    if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                    {
                        curr++;
                        if (curr == index)
                        {
                            m_Component.Children.Insert(i + 1, value);
                            return;
                        }
                    }
                }
            }
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            int curr = 0;
            for (int i = 0; i < m_Component.Children.Count; i++)
            {
                if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                {
                    if (curr++ == index)
                    {
                        m_Component.Children.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public T this[int index]
        {
            get
            {
                int curr = 0;
                for (int i = 0; i < m_Component.Children.Count; i++)
                {
                    if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                    {
                        if (curr++ == index)
                            return (T)m_Component.Children[i];                        
                    }
                }
                return default(T);
            }
            set
            {
                int curr = 0;
                for (int i = 0; i < m_Component.Children.Count; i++)
                {
                    if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                    {
                        if (curr++ == index)
                            m_Component.Children[i] = value;
                    }
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T value)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            m_Component.Children.Add(value);
        }

        public void Clear()
        {
            for (int i = m_Component.Children.Count - 1; i >= 0; i--)
            {
                if (m_Component.Children[i] is T && string.Equals(m_Component.Children[i].Name, m_ComponentName))
                    m_Component.Children.RemoveAt(i);
            }
        }

        public bool Contains(T value)
        {
            return IndexOf(value) >= 0;
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
                foreach (ICalendarComponent c in m_Component.Children)
                {
                    if (c is T && string.Equals(c.Name, m_ComponentName))
                        array[arrayIndex + index++] = (T)c;
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (ICalendarComponent c in m_Component.Children)
                {
                    if (c is T && string.Equals(c.Name, m_ComponentName))
                        count++;
                }
                return count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T value)
        {
            int index = IndexOf(value);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new CalendarComponentCompositeListEnumerator<T>(m_Component, m_ComponentName);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CalendarComponentCompositeListEnumerator<T>(m_Component, m_ComponentName);
        }

        #endregion

        private class CalendarComponentCompositeListEnumerator<T> :
            IEnumerator<T>            
        {
            #region Private Fields

            string m_ComponentName;
            ICalendarComponent m_Component;
            IEnumerator<ICalendarObject> m_Enumerator;

            #endregion

            #region Constructors

            public CalendarComponentCompositeListEnumerator(ICalendarComponent component, string componentName)
            {
                m_Component = component;
                m_ComponentName = componentName;
            }

            #endregion

            #region IEnumerator<ICalendarComponent> Members

            public T Current
            {
                get
                {
                    if (m_Enumerator != null)
                    {
                        object obj = m_Enumerator.Current;
                        if (obj is T)
                            return (T)obj;
                    }
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
                    return ((IEnumerator<ICalendarComponent>)this).Current;
                }
            }

            public bool MoveNext()
            {
                if (m_Enumerator == null)
                    m_Enumerator = m_Component.Children.GetEnumerator();

                while (m_Enumerator.MoveNext())
                {
                    if (m_Enumerator.Current is T)
                        return true;
                }
                                
                return false;
            }

            public void Reset()
            {
                m_Enumerator = null;
            }

            #endregion
        }
    }
}
