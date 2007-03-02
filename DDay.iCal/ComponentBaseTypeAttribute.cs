using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal
{
    public class ComponentBaseTypeAttribute : Attribute
    {
        public Type Type;

        public ComponentBaseTypeAttribute(Type ComponentBaseType)
        {
            if (ComponentBaseType == typeof(ComponentBase) ||
                ComponentBaseType.IsSubclassOf(typeof(ComponentBase)))
                Type = ComponentBaseType;
        }
    }
}
