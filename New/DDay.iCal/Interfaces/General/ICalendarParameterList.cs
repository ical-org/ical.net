using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarParameterList :
        IKeyedList<ICalendarParameter, string>
    {
        void Set(string name, string[] values);
        void Set(string name, string value);
        string[] Get(string name);
    }
}
