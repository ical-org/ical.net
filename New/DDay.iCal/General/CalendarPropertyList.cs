using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class CalendarPropertyList :
        KeyedList<ICalendarProperty, string>,
        ICalendarPropertyList
    {
        #region Private Fields

        ICalendarObject m_Parent;

        #endregion

        #region Constructors

        public CalendarPropertyList()
        {
        }

        public CalendarPropertyList(ICalendarObject parent)
        {
            m_Parent = parent;

            ItemAdded += new EventHandler<ObjectEventArgs<ICalendarProperty>>(CalendarPropertyList_ItemAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarProperty>>(CalendarPropertyList_ItemRemoved);
        }

        #endregion

        #region Event Handlers

        void CalendarPropertyList_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
            e.Object.Parent = null;
        }

        void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
            e.Object.Parent = m_Parent;
        }

        #endregion

        #region ICalendarPropertyList Members

        virtual public void Set(string name, object value)
        {
            if (name != null)
            {
                if (value != null)
                {
                    ICalendarProperty p = new CalendarProperty(name, value);
                    if (ContainsKey(name))
                        this[name] = p;
                    else
                        Add(p);
                }
                else
                {
                    Remove(name);
                }
            }
        }

        virtual public T Get<T>(string name)
        {
            if (name != null && ContainsKey(name))
            {
                object obj = this[name].Value;
                if (obj is T)
                    return (T)obj;
            }
            return default(T);
        }

        virtual public T[] GetAll<T>(string name)
        {
            if (name != null && ContainsKey(name))
            {
                List<T> objs = new List<T>();
                foreach (ICalendarProperty p in AllOf(name))
                {
                    object obj = p.Value;
                    if (obj is T)
                        objs.Add((T)obj);
                }
                return objs.ToArray();
            }
            return null;
        }

        #endregion
    }
}
