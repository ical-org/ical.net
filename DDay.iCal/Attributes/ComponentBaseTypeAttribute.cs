using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;

namespace DDay.iCal
{
    public class ComponentBaseTypeAttribute : Attribute
    {
        public Type Type;

        public ComponentBaseTypeAttribute(Type ComponentBaseType)
        {
            if (ComponentBaseType == typeof(CalendarComponent) ||
                ComponentBaseType.IsSubclassOf(typeof(CalendarComponent)))
                Type = ComponentBaseType;
        }
    }
}
