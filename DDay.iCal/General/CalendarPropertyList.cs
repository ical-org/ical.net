using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class CalendarPropertyList :        
        KeyedList<ICalendarProperty, string>,
        ICalendarPropertyList
    {
        #region ICalendarPropertyList Members

        public void Set(string name, object value)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string name)
        {
            throw new NotImplementedException();
        }

        public object Get(string name, Type returnType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
