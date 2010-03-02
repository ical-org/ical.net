using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataList<T> :
        IKeyedList<T, string>
        where T : ICalendarValueObject, IKeyedObject<string>
    {
        void Set(string name, object value);        
        T Get<T>(string name);
        object Get(string name, Type returnType);
        T[] GetAll<T>(string name);
        object[] GetAll(string name, Type returnType);
    }
}
