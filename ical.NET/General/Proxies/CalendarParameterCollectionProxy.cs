using System.Collections.Generic;
using System.Linq;
using ical.NET.Collections;
using ical.NET.Collections.Interfaces;
using ical.NET.Collections.Proxies;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.General.Proxies;

namespace Ical.Net.General.Proxies
{
    public class CalendarParameterCollectionProxy : GroupedCollectionProxy<string, CalendarParameter, CalendarParameter>, ICalendarParameterCollectionProxy
    {
        protected GroupedValueList<string, CalendarParameter, CalendarParameter, string> Parameters
            => RealObject as GroupedValueList<string, CalendarParameter, CalendarParameter, string>;

        public CalendarParameterCollectionProxy(IGroupedList<string, CalendarParameter> realObject) : base(realObject) {}

        public virtual void SetParent(ICalendarObject parent)
        {
            foreach (var parameter in this)
            {
                parameter.Parent = parent;
            }
        }

        public virtual void Add(string name, string value)
        {
            RealObject.Add(new CalendarParameter(name, value));
        }

        public virtual string Get(string name)
        {
            var parameter = RealObject.FirstOrDefault(o => o.Name == name);

            return parameter?.Value;
        }

        public virtual IList<string> GetMany(string name)
        {
            return new GroupedValueListProxy<string, CalendarParameter, CalendarParameter, string, string>(Parameters, name);
        }

        public virtual void Set(string name, string value)
        {
            var parameter = RealObject.FirstOrDefault(o => o.Name == name);

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, value));
            }
            else
            {
                parameter.SetValue(value);
            }
        }

        public virtual void Set(string name, IEnumerable<string> values)
        {
            var parameter = RealObject.FirstOrDefault(o => o.Name == name);

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, values));
            }
            else
            {
                parameter.SetValue(values);
            }
        }

        public virtual int IndexOf(CalendarParameter obj)
        {
            return 0;
        }

        public virtual void Insert(int index, CalendarParameter item) {}

        public virtual void RemoveAt(int index) {}

        public virtual CalendarParameter this[int index]
        {
            get { return Parameters[index]; }
            set { }
        }
    }
}