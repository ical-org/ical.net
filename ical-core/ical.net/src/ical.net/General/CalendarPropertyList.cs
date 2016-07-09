using System.Linq;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    public class CalendarPropertyList : GroupedValueList<string, ICalendarProperty, CalendarProperty, object>, ICalendarPropertyList
    {
        private readonly ICalendarObject _mParent;
        private readonly bool _mCaseInsensitive;

        public CalendarPropertyList() {}

        public CalendarPropertyList(ICalendarObject parent, bool caseInsensitive)
        {
            _mParent = parent;
            _mCaseInsensitive = caseInsensitive;
            ItemAdded += CalendarPropertyList_ItemAdded;
            ItemRemoved += CalendarPropertyList_ItemRemoved;
        }

        private void CalendarPropertyList_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = null;
        }

        private void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = _mParent;
        }

        protected override string GroupModifier(string group)
        {
            if (_mCaseInsensitive && group != null)
            {
                return group.ToUpper();
            }
            return group;
        }

        public ICalendarProperty this[string name]
        {
            get
            {
                if (ContainsKey(name))
                {
                    return AllOf(name).FirstOrDefault();
                }
                return null;
            }
        }
    }
}