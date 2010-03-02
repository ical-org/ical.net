using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;

namespace DDay.iCal
{
    public class ComponentFactoryAttribute : Attribute
    {
        public Type Type = null;

        public ComponentFactoryAttribute(Type factoryType)
        {
            if (typeof(ICalendarComponentFactory).IsAssignableFrom(factoryType))
                Type = factoryType;
        }
    }
}
