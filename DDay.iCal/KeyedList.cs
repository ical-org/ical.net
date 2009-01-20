using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    /// <summary>
    /// A list of objects that are keyed.  This is similar to a 
    /// Dictionary<T,U> object, except 
    /// </summary>
    public class KeyedList<T, U> :
        List<T>,
        IKeyedList<T, U> where T : IKeyedObject<U>
    {
        #region IKeyedList<T, U> Members

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
            return FindIndex(
                delegate(T ko)
                {
                    return object.Equals(ko.Key, key);
                }
            );
        }

        /// <summary>
        /// Gets/sets an object with the matching key to
        /// the provided value.  When setting the value,
        /// if another object with a matching key exists,
        /// it will be overwritten.  If overwriting is
        /// not desired, use the Add() method instead.
        /// </summary>        
        public T this[U key]
        {
            get
            {
                int index = IndexOf(key);
                if (index >= 0)
                    return this[IndexOf(key)];
                return default(T);
            }
            set
            {
                if (ContainsKey(key))
                    this[IndexOf(key)] = value;
                else
                    Add(value);
            }
        }       

        /// <summary>
        /// Removes all objects with the matching <paramref name="key"/>.
        /// </summary>
        /// <returns>True if any objects were removed, false otherwise.</returns>
        public bool Remove(U key)
        {
            int index = IndexOf(key);
            bool removed = false;

            while (index >= 0)
            {
                RemoveAt(index);
                removed = true;
                index = IndexOf(key);
            }

            return removed;
        }

        #endregion
    }
}
