using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public class CalendarParameterList :
        KeyedList<ICalendarParameter, string>,
        ICalendarParameterList
    {
        #region Private Fields

        ICalendarObject m_Parent;

        #endregion

        #region Constructors

        public CalendarParameterList()
        {
        }

        public CalendarParameterList(ICalendarObject parent)
        {
            m_Parent = parent;

            ItemAdded += new EventHandler<ObjectEventArgs<ICalendarParameter>>(CalendarParameterList_ItemAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarParameter>>(CalendarParameterList_ItemRemoved);
        }

        #endregion

        #region Event Handlers

        void CalendarParameterList_ItemRemoved(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            e.Object.Parent = null;
        }

        void CalendarParameterList_ItemAdded(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            e.Object.Parent = m_Parent;
        }

        #endregion

        #region ICalendarParameterList Members

        public void Set(string name, string[] values)
        {
            if (name != null)
            {
                if (ContainsKey(name))
                {
                    if (values != null)
                        this[name] = new CalendarParameter(name, values);
                    else
                        Remove(name);
                }
                Add(new CalendarParameter(name, values));
            }
        }

        public void Set(string name, string value)
        {
            Set(name, new string[] { value });
        }

        public string Get(string name)
        {
            if (name != null && ContainsKey(name))
                return this[name].Value;
            return null;
        }

        public string[] GetAll(string name)
        {
            if (name != null && ContainsKey(name))
            {
                List<string> values = new List<string>();
                foreach (ICalendarParameter p in AllOf(name))
                {
                    if (p.Values != null)
                        values.AddRange(p.Values);
                }
                return values.ToArray();
            }
            return null;
        }

        #endregion
    }
}
