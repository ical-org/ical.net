using System;
using System.Collections.Generic;
using Ical.Net.General;

namespace Ical.Net.Utility
{
    public static class KnownTypeHelper
    {
        public static IList<Type> GetKnownTypes() => new List<Type>
        {
            typeof(CalendarPropertyList),
            typeof(CalendarParameterList)
        };
    }
}