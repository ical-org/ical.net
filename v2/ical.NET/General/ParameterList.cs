using System.Collections.Generic;
using ical.net.collections;
using ical.net.Interfaces.General;

namespace ical.net.General
{
    public class ParameterList : GroupedValueList<string, CalendarParameter, CalendarParameter, string>, IParameterCollection
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