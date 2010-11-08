using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;

namespace DDay.iCal
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarParameterList :
        KeyedList<ICalendarParameter, string>,
        ICalendarParameterList
    {
        #region Private Fields

        /// <summary>
        /// NOTE: we use a weak reference here to ensure this doesn't cause a memory leak.
        /// As this class merely provides a service to calendar properties, we shouldn't
        /// be holding on to memory references via this object anyhow.
        /// </summary>
        WeakReference m_Parent;
        bool m_CaseInsensitive;

        #endregion

        #region Constructors

        public CalendarParameterList()
        {
        }

        public CalendarParameterList(ICalendarObject parent, bool caseInsensitive)
        {
            m_Parent = new WeakReference(parent);
            m_CaseInsensitive = caseInsensitive;

            ItemAdded += new EventHandler<ObjectEventArgs<ICalendarParameter>>(OnParameterAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarParameter>>(OnParameterRemoved);
        }

        #endregion

        #region Protected Methods

        protected void OnParameterRemoved(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            e.Object.Parent = null;
        }

        protected void OnParameterAdded(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            e.Object.Parent = m_Parent != null ? m_Parent as ICalendarObject : null;
        }

        #endregion

        #region ICalendarParameterList Members

        virtual public void Add(string name, string value)
        {
            if (name != null)
            {
                name = m_CaseInsensitive ? name.ToUpper() : name;
                Add(new CalendarParameter(name, value));
            }
        }

        virtual public void Add(string name, string[] values)
        {
            if (name != null)
            {
                name = m_CaseInsensitive ? name.ToUpper() : name;
                Add(new CalendarParameter(name, values));
            }
        }
        
        virtual public void Add(string name, IList<string> values)
        {
            if (name != null)
            {
                string[] vals = new string[values.Count];
                values.CopyTo(vals, 0);
                Add(new CalendarParameter(name, vals));
            }
        }

        virtual public void Set(string name, string[] values)
        {
            if (name != null)
            {
                name = m_CaseInsensitive ? name.ToUpper() : name;
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

        virtual public void Set(string name, string value)
        {
            Set(name, new string[] { value });
        }
        
        virtual public void Set(string name, IList<string> values)
        {
            string[] vals = new string[values.Count];
            values.CopyTo(vals, 0);
            Set(name, vals);
        }

        virtual public string Get(string name)
        {
            if (name != null)
            {
                name = m_CaseInsensitive ? name.ToUpper() : name;
                if (ContainsKey(name))
                    return this[name].Value;
            }
            return null;
        }

        virtual public string[] GetAll(string name)
        {
            if (name != null && ContainsKey(name))
            {
                name = m_CaseInsensitive ? name.ToUpper() : name;

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
        
        virtual public IList<string> GetList(string name)
        {
            name = m_CaseInsensitive ? name.ToUpper() : name;
            return new CalendarParameterCompositeList(this, name);
        }

        #endregion
    }
}
