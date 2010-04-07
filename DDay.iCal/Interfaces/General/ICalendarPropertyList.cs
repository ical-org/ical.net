using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarPropertyList : 
        IKeyedList<ICalendarProperty, string>
    {
        void Set(string name, object value);
        void SetList<U>(string name, IList<U> value);
        T Get<T>(string name);
        T[] GetAll<T>(string name);
        IList<U> GetList<U>(string name);
    }
}
