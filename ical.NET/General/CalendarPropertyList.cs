using System;
using System.Linq;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    [Serializable]
    public class CalendarPropertyList : GroupedValueList<string, ICalendarProperty, CalendarProperty, object>, ICalendarPropertyList
    {
        readonly ICalendarObject _mParent;
        bool _mCaseInsensitive;

        public CalendarPropertyList() {}

        public CalendarPropertyList(ICalendarObject parent, bool caseInsensitive)
        {
            _mParent = parent;

            ItemAdded += CalendarPropertyList_ItemAdded;
            ItemRemoved += CalendarPropertyList_ItemRemoved;
        }

        void CalendarPropertyList_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = null;
        }

        void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
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