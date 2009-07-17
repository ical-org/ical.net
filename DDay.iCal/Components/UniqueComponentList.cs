using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A collection of iCalendar components.  This class is used by the 
    /// <see cref="iCalendar"/> class to maintain a collection of events,
    /// to-do items, journal entries, and free/busy times.
    /// </summary>
#if SILVERLIGHT
    [DataContract(Name = "UniqueComponentList{0}", Namespace="http://www.ddaysoftware.com/dday.ical/components/2009/07/")]
#else
    [Serializable]
#endif
    public class UniqueComponentList<T> : iCalObject, IList<T>
    {
        #region Private Fields

        private List<T> m_Components = new List<T>();
        private Dictionary<string, T> m_Dictionary = new Dictionary<string, T>();

        #endregion

        #region Constructors

        public UniqueComponentList(iCalObject parent) : base(parent) { }
        public UniqueComponentList(iCalObject parent, string name) : base(parent, name) { }

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
                if ((item as UniqueComponent).UID == null)
                    (item as UniqueComponent).UID = UniqueComponent.NewUID();                
                m_Dictionary[(item as UniqueComponent).UID] = item;
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
            if ((item as UniqueComponent).UID != null)
                m_Dictionary[(item as UniqueComponent).UID] = item;
        }

        public void RemoveAt(int index)
        {
            T item = m_Components[index];
            if ((item as UniqueComponent).UID != null)
                m_Dictionary.Remove((item as UniqueComponent).UID);
            m_Components.RemoveAt(index);
        }

        public void UIDChangedHandler(object sender, Text oldUID, Text newUID)
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
                if ((item as UniqueComponent).UID != null)
                {
                    m_Dictionary.Remove((item as UniqueComponent).UID);
                    m_Dictionary[(value as UniqueComponent).UID] = value;
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

                UniqueComponent uc = item as UniqueComponent;
                uc.UIDChanged += new UniqueComponent.UIDChangedEventHandler(UIDChangedHandler);

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
            return m_Components.Remove(item) && ((item as UniqueComponent).UID == null || m_Dictionary.Remove((item as UniqueComponent).UID));
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
