using System.Collections.Generic;

namespace ical.net.collections.Interfaces
{
    public interface IValueObject<T>
    {
        IEnumerable<T> Values { get; }

        bool ContainsValue(T value);
        void SetValue(T value);
        void SetValue(IEnumerable<T> values);
        void AddValue(T value);
        void RemoveValue(T value);
        int ValueCount { get; }
    }
}
