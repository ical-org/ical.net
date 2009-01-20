using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IKeyedList<T, U> :
        IList<T> where T : IKeyedObject<U>
    {
        bool ContainsKey(U key);
        int IndexOf(U key);
        T this[U key] { get; set; }
        bool Remove(U key);
    }
}
