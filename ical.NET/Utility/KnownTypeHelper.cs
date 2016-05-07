using System;
using System.Collections.Generic;
using Ical.Net.General;

namespace Ical.Net.Utility
{
    public static class KnownTypeHelper
    {
        public static IList<Type> GetKnownTypes()
        {
            var types = new List<Type>();

            types.Add(typeof(CalendarPropertyList));
            types.Add(typeof(CalendarParameterList));

            return types;
        }
    }
}
