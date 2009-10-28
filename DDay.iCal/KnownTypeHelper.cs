using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using System.Reflection;

namespace DDay.iCal
{
    static public class KnownTypeHelper
    {
        static public IList<Type> GetKnownTypes()
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(KeyedList<Property, string>));
            types.Add(typeof(KeyedList<Parameter, string>));
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
