using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    public class FilteredCalendarObjectList<T> :
        IFilteredCalendarObjectList<T>
        where T : ICalendarObject
    {
        #region Public Events

        public event EventHandler<ObjectEventArgs<T>> ItemAdded;
        public event EventHandler<ObjectEventArgs<T>> ItemRemoved;

        protected void OnItemAdded(T item)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<T>(item));
        }

        protected void OnItemRemoved(T item)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<T>(item));
        }

        #endregion

        #region Private Fields

        private ICalendarObject m_Attached = null;
        private List<T> m_Items = null;

        #endregion

        #region Protected Properties

        protected ICalendarObject Attached
        {
            get { return m_Attached; }
            set { m_Attached = value; }
        }

        #endregion

        #region Constructors

        public FilteredCalendarObjectList(ICalendarObject attached)
        {
            m_Attached = attached;
            m_Items = new List<T>();
            m_Attached.ChildAdded += new EventHandler<ObjectEventArgs<ICalendarObject>>(m_Attached_ChildAdded);
            m_Attached.ChildRemoved += new EventHandler<ObjectEventArgs<ICalendarObject>>(m_Attached_ChildRemoved);
        }        

        #endregion

        #region Event Handlers

        void m_Attached_ChildRemoved(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (e.Object is T)
            {
                T item = (T)e.Object;
                m_Items.Remove(item);
                OnItemRemoved(item);
            }
        }

        void m_Attached_ChildAdded(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (e.Object is T)
            {
                T item = (T)e.Object;
                m_Items.Add(item);
                OnItemAdded(item);
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            m_Attached.Children.Add(item);            
        }

        public void Clear()
        {
            foreach (T item in m_Items)
                m_Attached.Children.Remove(item);
        }

        public bool Contains(T item)
        {
            return m_Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_Items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return m_Attached.Children.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        #endregion

        #region IFilteredCalendarObjectList<T> Members

        public T this[int index]
        {
            get
            {
                return m_Items[index];
            }
        }

        public int IndexOf(T item)
        {
            return m_Items.IndexOf(item);
        }

        #endregion
    }
}
