using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A collection of iCalendar components.  This class is used by the 
    /// <see cref="iCalendar"/> class to maintain a collection of events,
    /// to-do items, journal entries, and free/busy times.
    /// </summary>
#if DATACONTRACT
    [DataContract(Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class UniqueComponentList<T> : IUniqueComponentList<T> where T : IUniqueComponent
    {
        #region Private Fields

        private List<T> m_Components = new List<T>();
        private Dictionary<string, T> m_Dictionary = new Dictionary<string, T>();

        #endregion

        #region Public Members

        /// <summary>
        /// Re-links the UID dictionary to the actual components in our list.
        /// Also, if any items do not have a UID assigned to them, they will
        /// automatically have a UID assigned.
        /// </summary> 
        public void ResolveUIDs()
        {
            m_Dictionary.Clear();
            foreach (T item in m_Components)
            {
                if (item.UID == null)
                    item.UID = UniqueComponent.NewUID();
                m_Dictionary[item.UID] = item;
            }
        }

        public bool ContainsKey(string UID)
        {
            return m_Dictionary.ContainsKey(UID);
        }
                
        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return m_Components.IndexOf(item);
        }

        public void Insert(int index, T item)
        {   
            m_Components.Insert(index, item);
            if (item.UID != null)
                m_Dictionary[item.UID] = item;
        }

        public void RemoveAt(int index)
        {
            T item = m_Components[index];
            if (item.UID != null)
                m_Dictionary.Remove(item.UID);
            m_Components.RemoveAt(index);
        }

        public void UIDChangedHandler(object sender, IText oldUID, IText newUID)
        {
            if (oldUID != null && ContainsKey(oldUID))
                m_Dictionary.Remove(oldUID);
            if (newUID != null)
                m_Dictionary[newUID] = (T)sender;
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0 &&
                    index < m_Components.Count)
                    return m_Components[index];
                return default(T);
            }
            set
            {
                T item = m_Components[index];
                if (item.UID != null)
                {
                    m_Dictionary.Remove(item.UID);
                    m_Dictionary[(value as IUniqueComponent).UID] = value;
                }
                m_Components[index] = value;
            }
        }

        public T this[string uid]
        {
            get
            {
                if (m_Dictionary.ContainsKey(uid))
                    return m_Dictionary[uid];
                return default(T);
            }
            set
            {                
                if (m_Dictionary.ContainsKey(uid))
                {
                    T item = this[uid];
                    Remove(item);
                    Add(value);
                }
                else
                {
                    Add(value);                    
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {            
            if (!m_Components.Contains(item))
            {                
                m_Components.Add(item);

                IUniqueComponent uc = item as IUniqueComponent;
                uc.UIDChanged += new UIDChangedEventHandler(UIDChangedHandler);

                if (uc.UID != null)
                    m_Dictionary[uc.UID] = item;
            }
        }

        public bool Contains(T item)
        {
            return m_Components.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_Components.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return m_Components.Remove(item) && (item.UID == null || m_Dictionary.Remove(item.UID));
        }

        public void Clear()
        {
            m_Components.Clear();
            m_Dictionary.Clear();
        }

        public int Count
        {
            get { return m_Components.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_Components.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return m_Components.GetEnumerator();
        }

        #endregion
    }
}
