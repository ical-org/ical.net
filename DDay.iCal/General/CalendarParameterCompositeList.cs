using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// This class works similar to the CalendarPropertyCompositeList class,
    /// but works with parameters instead of properties.
    /// 
    /// It consolidates parameters of a given name into a list,
    /// and allows you to work with those values directly against the
    /// parameters themselves.  This preserves the notion that our values
    /// are still stored directly within the calendar parameter, but gives
    /// us the flexibility to work with multiple parameters through a
    /// single (composite) list.
    /// </summary>
    public class CalendarParameterCompositeList :
        ICalendarParameterCompositeList
    {
        #region Private Fields

        ICalendarParameterList m_ParameterList;
        string m_ParameterName;

        #endregion

        #region Constructors

        public CalendarParameterCompositeList(ICalendarParameterList parameterList, string parameterName)
        {
            m_ParameterList = parameterList;
            m_ParameterName = parameterName;
        }

        #endregion

        #region Protected Methods        
        
        protected ICalendarParameter ParameterForIndex(int index, out int parameterIndex, out int indexInParameter)
        {
            int count = 0;
            parameterIndex = 0;            

            // Search through the parameters for the one
            // that contains the index in question.
            int parameterValueCount = 0;
            while (parameterIndex < m_ParameterList.Count)
            {
                if (string.Equals(m_ParameterList[parameterIndex].Name, m_ParameterName) && m_ParameterList[parameterIndex].Values != null)
                {
                    parameterValueCount = m_ParameterList[parameterIndex].Values.Length;
                    if (parameterValueCount > 0) // The value contains items
                    {
                        if (count + parameterValueCount > index) // The desired index is in this parameter
                            break;

                        count += parameterValueCount;
                    }
                }
                parameterIndex++;
            }

            if (parameterIndex < m_ParameterList.Count &&
                count <= index &&
                parameterValueCount > index - count)
            {
                indexInParameter = index - count;
                return m_ParameterList[parameterIndex];
            }

            parameterIndex = -1;
            indexInParameter = -1;
            return null;
        }

        protected ICalendarParameter ParameterForValue(string value, out int itemIndex, out int indexInParameter)
        {
            itemIndex = 0;
            foreach (ICalendarParameter p in m_ParameterList)
            {
                if (p.Name.Equals(m_ParameterList))
                {
                    if (p.Values != null)
                    {
                        List<string> list = new List<string>(p.Values);  
                        indexInParameter = list.IndexOf(value);
                        if (indexInParameter >= 0)
                        {                            
                            itemIndex += indexInParameter;
                            return p;
                        }
                        else itemIndex += list.Count;
                    }                    
                }
            }
            indexInParameter = -1;
            itemIndex = -1;
            return null;
        }

        #endregion

        #region IList<string> Members

        public int IndexOf(string value)
        {
            int itemIndex, indexInParameter;
            ICalendarParameter p = ParameterForValue(value, out itemIndex, out indexInParameter);
            return itemIndex;
        }

        public void Insert(int index, string value)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            int parameterIndex, indexInParameter;
            ICalendarParameter p = ParameterForIndex(index, out parameterIndex, out indexInParameter);
            if (p != null)
            {
                List<string> values = new List<string>(p.Values);
                values.Insert(indexInParameter, value);
                p.Values = values.ToArray();
            }
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException();
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            int parameterIndex, indexInParameter;
            ICalendarParameter p = ParameterForIndex(index, out parameterIndex, out indexInParameter);
            if (p != null)
            {
                List<string> values = new List<string>(p.Values);
                values.RemoveAt(indexInParameter);
                p.Values = values.ToArray();
            }
        }

        public string this[int index]
        {
            get
            {
                int parameterIndex, indexInParameter;
                ICalendarParameter p = ParameterForIndex(index, out parameterIndex, out indexInParameter);
                if (p != null)
                    return p.Values[indexInParameter];
                return null;
            }
            set
            {
                int parameterIndex, indexInParameter;
                ICalendarParameter p = ParameterForIndex(index, out parameterIndex, out indexInParameter);
                if (p != null)
                {
                    List<string> values = new List<string>(p.Values);
                    values[indexInParameter] = value;
                    p.Values = values.ToArray();
                    // FIXME: Call OnItemAdded()/OnItemRemoved() here?
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(string value)
        {
            if (IsReadOnly)
                throw new NotSupportedException();

            // Add the value to a pre-existing parameter, if possible
            foreach (ICalendarParameter p in m_ParameterList)
            {
                if (string.Equals(p.Name, m_ParameterName))
                {
                    List<string> values = new List<string>(p.Values);
                    values.Add(value);
                    p.Values = values.ToArray();
                    return;
                }
            }

            // Create a new parameter to hold our value
            ICalendarParameter parm = new CalendarParameter();
            parm.Name = m_ParameterName;
            parm.Values = new string[] { value };

            m_ParameterList.Add(parm);
        }

        public void Clear()
        {
            for (int i = m_ParameterList.Count - 1; i >= 0; i--)
            {
                if (string.Equals(m_ParameterList[i].Name, m_ParameterName))
                    m_ParameterList.RemoveAt(i);
            }
        }

        public bool Contains(string value)
        {
            return IndexOf(value) >= 0;
        }

        public void CopyTo(string[] array, int arrayIndex)
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
                foreach (ICalendarParameter p in m_ParameterList)
                {
                    if (string.Equals(p.Name, m_ParameterName))
                    {
                        p.Values.CopyTo(array, index + arrayIndex);
                        index += p.Values.Length;                        
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (ICalendarParameter p in m_ParameterList)
                {
                    if (string.Equals(p.Name, m_ParameterName))
                        count += p.Values.Length;
                }
                return count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(string value)
        {
            int itemIndex, indexInParameter;
            ICalendarParameter p = ParameterForValue(value, out itemIndex, out indexInParameter);
            if (p != null &&
                p.Values != null)
            {
                if (p.Values.Length == 1 && indexInParameter == 0)
                {
                    // Remove the parameter itself
                    m_ParameterList.RemoveAt(itemIndex);
                }
                else
                {
                    // Remove the value from the parameter
                    List<string> values = new List<string>(p.Values);
                    values.RemoveAt(indexInParameter);
                    p.Values = values.ToArray();
                }
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new CalendarParameterCompositeListEnumerator(m_ParameterList, m_ParameterName);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CalendarParameterCompositeListEnumerator(m_ParameterList, m_ParameterName);
        }

        #endregion

        private class CalendarParameterCompositeListEnumerator :
            IEnumerator<string>
        {
            #region Private Fields

            string m_ParameterName;
            ICalendarParameterList m_ParameterList;
            IEnumerator m_CurrentListEnumerator;
            int m_ParameterIndex = -1;

            #endregion

            #region Constructors

            public CalendarParameterCompositeListEnumerator(ICalendarParameterList parameterList, string parameterName)
            {
                m_ParameterList = parameterList;
                m_ParameterName = parameterName;
            }

            #endregion

            #region Private Methods

            private void MoveNextParameter()
            {
                m_CurrentListEnumerator = null;
                while (m_ParameterIndex + 1 < m_ParameterList.Count)
                {
                    ICalendarParameter p = m_ParameterList[++m_ParameterIndex];
                    if (string.Equals(p.Name, m_ParameterName))
                    {
                        List<string> list = new List<string>(p.Values);                        
                        m_CurrentListEnumerator = list.GetEnumerator();
                        return;
                    }
                }
            }

            #endregion

            #region IEnumerator<string> Members

            public string Current
            {
                get
                {
                    if (m_CurrentListEnumerator != null)
                        return ((IEnumerator<string>)m_CurrentListEnumerator).Current;
                    return null;
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
                    return ((IEnumerator<string>)this).Current;
                }
            }

            public bool MoveNext()
            {
                if (m_CurrentListEnumerator == null)
                    MoveNextParameter();

                if (m_CurrentListEnumerator != null)
                {
                    if (!m_CurrentListEnumerator.MoveNext())
                    {
                        MoveNextParameter();
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
                m_ParameterIndex = -1;
            }

            #endregion
        }
    }
}
