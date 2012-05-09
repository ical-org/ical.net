using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        FilteredCalendarObjectList<IUniqueComponent>,
        IUniqueComponentList<T>
        where T : class, IUniqueComponent
    {
        #region Private Fields

        private UIDFactory m_UIDFactory = new UIDFactory();

        #endregion

        #region Constructors

        public UniqueComponentList(ICalendarObject attached) : base(attached)
        {
            ResolveUIDs();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Automatically assigns a UID to items that do not already have one.
        /// </summary>
        public void ResolveUIDs()
        {
            foreach (T item in this)
            {
                if (string.IsNullOrEmpty(item.UID))
                    item.UID = m_UIDFactory.Build();
            }
        }
                        
        #endregion

        #region IKeyedList<string,T> Members

        public event EventHandler<ObjectEventArgs<T>> ItemAdded;

        public event EventHandler<ObjectEventArgs<T>> ItemRemoved;

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear(string key)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public int CountOf(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Values()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> AllOf(string key)
        {
            throw new NotImplementedException();
        }

        public T this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
