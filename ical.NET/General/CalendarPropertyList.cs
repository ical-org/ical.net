using System;
using System.Linq;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarPropertyList :
        GroupedValueList<string, ICalendarProperty, CalendarProperty, object>,
        ICalendarPropertyList
    {
        #region Private Fields

        ICalendarObject _mParent;
        bool _mCaseInsensitive;

        #endregion

        #region Constructors

        public CalendarPropertyList()
        {
        }

        public CalendarPropertyList(ICalendarObject parent, bool caseInsensitive)
        {
            _mParent = parent;

            ItemAdded += new EventHandler<ObjectEventArgs<ICalendarProperty, int>>(CalendarPropertyList_ItemAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarProperty, int>>(CalendarPropertyList_ItemRemoved);
        }

        #endregion
        
        #region Event Handlers

        void CalendarPropertyList_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = null;
        }

        void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = _mParent;
        }

        #endregion

        protected override string GroupModifier(string group)
        {
            if (_mCaseInsensitive && group != null)
                return group.ToUpper();
            return group;
        }

        public ICalendarProperty this[string name]
        {
            get
            {
                if (ContainsKey(name))
                {
                    return AllOf(name)
                        .FirstOrDefault();
                }
                return null;
            }            
        }
    }
}
