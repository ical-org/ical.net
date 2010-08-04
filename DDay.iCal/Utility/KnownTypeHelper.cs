using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DDay.iCal
{
    static public class KnownTypeHelper
    {
        static public IList<Type> GetKnownTypes()
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(CalendarPropertyList));
            types.Add(typeof(CalendarParameterList));            
            
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
