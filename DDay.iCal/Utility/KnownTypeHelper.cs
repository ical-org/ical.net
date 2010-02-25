using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using DDay.iCal;
using System.Reflection;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    static public class KnownTypeHelper
    {
        static public IList<Type> GetKnownTypes()
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(KeyedList<CalendarProperty, string>));
            types.Add(typeof(KeyedList<CalendarParameter, string>));
            types.Add(typeof(UniqueComponentList<UniqueComponent>));
            types.Add(typeof(UniqueComponentList<Event>));
            types.Add(typeof(UniqueComponentList<Todo>));
            types.Add(typeof(UniqueComponentList<Journal>));
            types.Add(typeof(Event));
            types.Add(typeof(Todo));
            types.Add(typeof(Journal));
            types.Add(typeof(FreeBusy));
            types.Add(typeof(Alarm));            

            return types;
        }
    }
}
