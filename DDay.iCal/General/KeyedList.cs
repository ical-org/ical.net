using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A list of objects that are keyed.  This is similar to a 
    /// Dictionary<T,U> object, except 
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class KeyedList<T, U> :
        IKeyedList<T, U> where T : IKeyedObject<U>
    {
        #region Private Fields

        List<T> _Items = new List<T>();

        #endregion

        #region IKeyedList<T, U> Members

        public event EventHandler<ObjectEventArgs<T>> ItemAdded;
        public event EventHandler<ObjectEventArgs<T>> ItemRemoved;

        protected void OnItemAdded(T obj)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<T>(obj));
        }

        protected void OnItemRemoved(T obj)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<T>(obj));
        }

        /// <summary>
        /// Returns true if the list contains at least one 
        /// object with a matching key, false otherwise.
        /// </summary>
        public bool ContainsKey(U key)
        {
            return IndexOf(key) >= 0;
        }

        /// <summary>
        /// Returns the index of the first object
        /// with the matching key.
        /// </summary>
        public int IndexOf(U key)
        {
            return _Items.FindIndex(
                delegate(T obj)
                {
                    return object.Equals(obj.Key, key);
                }
            );
        }

        public int CountOf(U key)
        {
            return AllOf(key).Count;
        }

        public IList<T> AllOf(U key)
        {
            return _Items.FindAll(
                delegate(T obj)
                {
                    return object.Equals(obj.Key, key);
                }
            );
        }

        public T this[U key]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    T obj = _Items[i];
                    if (object.Equals(obj.Key, key))
                        return obj;
                }
                return default(T);
            }
            set
            {
                int index = IndexOf(key);
                if (index >= 0)
                {
                    OnItemRemoved(_Items[index]);
                    _Items[index] = value;
                    OnItemAdded(value);
                }
                else
                {
                    Add(value);
                }
            }
        }

        public bool Remove(U key)
        {
            int index = IndexOf(key);
            bool removed = false;

            while (index >= 0)
            {
                T item = _Items[index];
                RemoveAt(index);
                OnItemRemoved(item);
                removed = true;
                index = IndexOf(key);
            }

            return removed;
        }

        #endregion

        #region IKeyedList<T,U> Members

        public T[] ToArray()
        {
            return _Items.ToArray();
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _Items.Insert(index, item);
            OnItemAdded(item);
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                T item = _Items[index];
                _Items.RemoveAt(index);
                OnItemRemoved(item);
            }
        }

        public T this[int index]
        {
            get
            {
                return _Items[index];
            }
            set
            {
                if (index >= 0 && index < Count)
                {
                    T item = _Items[index];                    
                    _Items[index] = value;
                    OnItemRemoved(item);
                    OnItemAdded(value);
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            _Items.Add(item);
            OnItemAdded(item);
        }

        public void Clear()
        {
            foreach (T obj in _Items)
                OnItemRemoved(obj);
            _Items.Clear();
        }

        public bool Contains(T item)
        {
            return _Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _Items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            bool removed = _Items.Remove(item);
            OnItemRemoved(item);
            return removed;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        #endregion
    }
}
