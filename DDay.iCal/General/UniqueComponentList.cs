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
#if !SILVERLIGHT
    [Serializable]
#endif
    public class UniqueComponentList<T> : 
        FilteredCalendarObjectList<T>,
        IUniqueComponentList<T>
        where T : IUniqueComponent
    {
        #region Private Fields

        private UIDFactory m_UIDFactory = new UIDFactory();
        private Dictionary<string, T> m_Dictionary = new Dictionary<string, T>();

        #endregion

        #region Constructors

        public UniqueComponentList(ICalendarObject attached) : base(attached)
        {
            ResolveUIDs();
            ItemAdded += new EventHandler<ObjectEventArgs<T>>(UniqueComponentList_ItemAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<T>>(UniqueComponentList_ItemRemoved);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Re-links the UID dictionary to the actual components in our list.
        /// Also, if any items do not have a UID assigned to them, they will
        /// automatically have a UID assigned.
        /// </summary>
        public void ResolveUIDs()
        {
            m_Dictionary.Clear();
            foreach (T item in this)
                HandleItemAdded(item);
        }

        public bool ContainsKey(string UID)
        {
            return m_Dictionary.ContainsKey(UID);
        }
                
        #endregion

        #region Protected Methods
        
        protected void HandleItemAdded(T item)
        {
            // Assign the item a UID if it's loaded.
            if (item.IsLoaded && item.UID == null)
                item.UID = m_UIDFactory.Build();
            
            item.UIDChanged += UIDChangedHandler;
            if (item.UID != null)
                m_Dictionary[item.UID] = item;
        }

        protected void HandleItemRemoved(T item)
        {
            item.UIDChanged -= UIDChangedHandler;
            if (item.UID != null && m_Dictionary.ContainsKey(item.UID))
                m_Dictionary.Remove(item.UID);
        }

        #endregion

        #region Event Handlers

        protected void UIDChangedHandler(object sender, string oldUID, string newUID)
        {
            if (oldUID != null && ContainsKey(oldUID))
                m_Dictionary.Remove(oldUID);
            if (newUID != null)
                m_Dictionary[newUID] = (T)sender;
        }

        void UniqueComponentList_ItemRemoved(object sender, ObjectEventArgs<T> e)
        {
            HandleItemRemoved(e.Object);
        }

        void UniqueComponentList_ItemAdded(object sender, ObjectEventArgs<T> e)
        {
            HandleItemAdded(e.Object);
        }

        #endregion

        #region IUniqueComponentList<T> Members

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
    }
}
