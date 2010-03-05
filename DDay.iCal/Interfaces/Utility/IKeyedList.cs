using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IKeyedList<T, U> :
        IList<T> where T : IKeyedObject<U>
    {
        event EventHandler<KeyedObjectEventArgs<T>> ItemAdded;
        event EventHandler<KeyedObjectEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Returns true if the list contains at least one 
        /// object with a matching key, false otherwise.
        /// </summary>
        bool ContainsKey(U key);

        /// <summary>
        /// Returns the index of the first object
        /// with the matching key.
        /// </summary>
        int IndexOf(U key);

        /// <summary>
        /// Returns the number of objects in the list
        /// with a matching key.
        /// </summary>
        int CountOf(U key);

        /// <summary>
        /// Returns an enumerable list of objects that
        /// match the specified key.
        /// </summary>
        IList<T> AllOf(U key);

        /// <summary>
        /// Gets/sets an object with the matching key to
        /// the provided value.  When setting the value,
        /// if another object with a matching key exists,
        /// it will be overwritten.  If overwriting is
        /// not desired, use the Add() method instead.
        /// </summary>
        T this[U key] { get; set; }

        /// <summary>
        /// Removes all objects with the matching <paramref name="key"/>.
        /// </summary>
        /// <returns>True if any objects were removed, false otherwise.</returns>
        bool Remove(U key);

        /// <summary>
        /// Converts the list to an array of the values contained therein.
        /// </summary>
        T[] ToArray();
    }

    public class KeyedObjectEventArgs<T> :
        EventArgs
    {
        public T Object { get; set; }

        public KeyedObjectEventArgs(T obj)
        {
            Object = obj;
        }
    }
}
