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
    public class FilteredCalendarObjectList<T> :
        ICalendarObjectList<T>
        where T : class, ICalendarObject
    {
        #region Protected Properties

        protected ICalendarObject Attached { get; set; }

        #endregion

        #region Constructors

        public FilteredCalendarObjectList(ICalendarObject attached)
        {
            Attached = attached;
            Attached.Children.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarObject>>(Children_ItemAdded);
            Attached.Children.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarObject>>(Children_ItemRemoved);
        }

        void Children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (e.Object is T)
                OnItemRemoved(this, e);
        }

        void Children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (e.Object is T)
                OnItemAdded(this, e);
        }

        #endregion

        #region IKeyedList<string,T> Members

        public event EventHandler<ObjectEventArgs<T>> ItemAdded;
        public event EventHandler<ObjectEventArgs<T>> ItemRemoved;

        protected void OnItemAdded(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (ItemAdded != null)
                ItemAdded(sender, new ObjectEventArgs<T>((T)e.Object));
        }

        protected void OnItemRemoved(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            if (ItemRemoved != null)
                ItemRemoved(sender, new ObjectEventArgs<T>((T)e.Object));
        }

        virtual public void Add(T item)
        {
            Attached.AddChild(item);
        }

        virtual public void Insert(int index, T item)
        {
            Attached.Children.Insert(index, item);
        }

        virtual public bool Remove(T item)
        {
            return Attached.Children.Remove(item);
        }

        virtual public bool Remove(string key)
        {
            return Attached.Children.Remove(key);
        }

        virtual public int IndexOf(T item)
        {
            return Attached.Children.IndexOf(item);
        }

        virtual public void Clear(string key)
        {
            // Clear all items matching the given key, of the type filtered in this list.
            var items = Attached.Children.AllOf(key).OfType<T>().ToArray();
            foreach (var item in items)
                Attached.Children.Remove(item);
        }

        virtual public void Clear()
        {
            // Clear all items of the type filtered in this list.
            var items = Attached.Children.OfType<T>().ToArray();
            foreach (var item in items)
                Attached.Children.Remove(item);
        }
        
        virtual public bool ContainsKey(string key)
        {
            return Attached.Children.ContainsKey(key);
        }

        virtual public int CountOf(string key)
        {
            if (Attached.Children.ContainsKey(key))
                return Attached.Children.AllOf(key).OfType<T>().Count();
            return 0;
        }

        virtual public IEnumerable<T> Values()
        {
            return Attached.Children.OfType<T>();
        }

        virtual public IEnumerable<T> AllOf(string key)
        {
            return Attached.Children.AllOf(key).OfType<T>();
        }

        virtual public T this[string key]
        {
            get
            {
                return Attached.Children[key] as T;
            }
            set
            {
                Attached.Children[key] = value;
            }
        }

        virtual public T[] ToArray()
        {
            return Attached.Children.Values().OfType<T>().ToArray();            
        }

        #endregion

        #region IEnumerable<T> Members

        virtual public IEnumerator<T> GetEnumerator()
        {
            return Attached.Children.Values().OfType<T>().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Attached.Children.Values().OfType<T>().GetEnumerator();
        }

        #endregion
    }
}
