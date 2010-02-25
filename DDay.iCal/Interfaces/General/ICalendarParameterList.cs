using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarParameterList :
        IKeyedList<ICalendarParameter, string>
    {
        void Set(string name, object value);
        T Get<T>(string name);
        object Get(string name, Type returnType);
    }
}
