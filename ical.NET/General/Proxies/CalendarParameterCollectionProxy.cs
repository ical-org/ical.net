using System.Collections.Generic;
using System.Linq;
using ical.NET.Collections.Interfaces;
using ical.NET.Collections.Proxies;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.General.Proxies;

namespace Ical.Net.General.Proxies
{
    public class CalendarParameterCollectionProxy : GroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>, ICalendarParameterCollectionProxy
    {
        protected IGroupedValueList<string, ICalendarParameter, CalendarParameter, string> Parameters
            => RealObject as IGroupedValueList<string, ICalendarParameter, CalendarParameter, string>;

        public CalendarParameterCollectionProxy(IGroupedList<string, ICalendarParameter> realObject) : base(realObject) {}

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
            return new GroupedValueListProxy<string, ICalendarParameter, CalendarParameter, string, string>(Parameters, name);
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

        public virtual int IndexOf(ICalendarParameter obj)
        {
            return Parameters.IndexOf(obj);
        }

        public virtual void Insert(int index, ICalendarParameter item)
        {
            Parameters.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            Parameters.RemoveAt(index);
        }

        public virtual ICalendarParameter this[int index]
        {
            get { return Parameters[index]; }
            set { }
        }
    }
}