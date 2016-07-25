﻿using System.Collections.Generic;
using ical.Net.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    public class CalendarParameterList : GroupedValueList<string, ICalendarParameter, CalendarParameter, string>, ICalendarParameterCollection
    {
        public virtual void SetParent(ICalendarObject parent)
        {
            foreach (var parameter in this)
            {
                parameter.Parent = parent;
            }
        }

        public virtual void Add(string name, string value)
        {
            Add(new CalendarParameter(name, value));
        }

        public virtual string Get(string name) => Get<string>(name);

        public virtual IList<string> GetMany(string name) => GetMany<string>(name);
    }
}