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
            types.Add(typeof(KeyedList<ICalendarProperty, string>));
            types.Add(typeof(KeyedList<ICalendarParameter, string>));
            types.Add(typeof(UniqueComponentList<IUniqueComponent>));
            types.Add(typeof(UniqueComponentList<IEvent>));
            types.Add(typeof(UniqueComponentList<ITodo>));
            types.Add(typeof(UniqueComponentList<IJournal>));
            
            // FIXME: re-add known types
            /*types.Add(typeof(Event));
            types.Add(typeof(Todo));
            types.Add(typeof(Journal));
            types.Add(typeof(FreeBusy));
            types.Add(typeof(Alarm));            */

            return types;
        }
    }
}
