using System;
using System.Collections.Generic;

namespace ical.NET.Collections
{
    public class ObjectEventArgs<T> :
        EventArgs
    {
        public T Object { get; set; }

        public ObjectEventArgs(T obj)
        {
            Object = obj;
        }
    }

    public class ObjectEventArgs<T, TU> :
        EventArgs
    {
        public T First { get; set; }
        public TU Second { get; set; }

        public ObjectEventArgs(T first, TU second)
        {
            First = first;
            Second = second;
        }
    }

    public class ValueChangedEventArgs<T> :
        EventArgs
    {
        public IEnumerable<T> RemovedValues { get; protected set; }
        public IEnumerable<T> AddedValues { get; protected set; }

        public ValueChangedEventArgs(IEnumerable<T> removedValues, IEnumerable<T> addedValues)
        {
            RemovedValues = removedValues ?? new T[0];
            AddedValues = addedValues ?? new T[0];
        }
    }
}
