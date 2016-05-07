using System.Collections.Generic;
using ical.NET.Collections.Interfaces;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarParameterCollection : IGroupedList<string, ICalendarParameter>
    {
        void SetParent(ICalendarObject parent);
        void Add(string name, string value);
        string Get(string name);
        IList<string> GetMany(string name);
        void Set(string name, string value);
        void Set(string name, IEnumerable<string> values);
    }
}