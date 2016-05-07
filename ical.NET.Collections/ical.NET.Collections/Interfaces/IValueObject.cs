using System;
using System.Collections.Generic;

namespace ical.NET.Collections.Interfaces
{
    public interface IValueObject<T>
    {
        event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
        IEnumerable<T> Values { get; }

        bool ContainsValue(T value);
        void SetValue(T value);
        void SetValue(IEnumerable<T> values);
        void AddValue(T value);
        void RemoveValue(T value);
        int ValueCount { get; }
    }
}
