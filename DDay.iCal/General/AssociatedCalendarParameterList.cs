using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// This class provides a parameter list via the associated container.
    /// <example>
    /// For example, let's say an event has several properties:
    /// 
    /// BEGIN:VEVENT
    /// SUMMARY:My Event
    /// DTSTART;TZID=MST:20100703T080000
    /// DTEND;TZID=MST:20100703T090000
    /// END:VEVENT
    /// 
    /// When we process this event, we get an object model similar to this:
    /// 
    /// Event
    ///   -> Properties
    ///         -> CalendarProperty
    ///             -> Name: SUMMARY
    ///             -> Value: My Event
    ///         -> CalendarProperty
    ///             -> Name: DTSTART
    ///             -> Value
    ///                 -> Type: iCalDateTime
    ///                 -> Value: 20100703T080000
    ///             -> Parameters
    ///                 -> CalendarParameter: TZID=MST
    ///         -> CalendarProperty
    ///             -> Name: DTEND
    ///             -> Value
    ///                 -> Type: iCalDateTime
    ///                 -> Value: 20100703T090000
    ///             -> Parameters
    ///                 -> CalendarParameter: TZID=MST
    /// </example>
    /// The problem with this is that, in order to serialize the value
    /// of DTSTART and DTEND properly, they need to be aware of the
    /// TZID parameters that were attached to the event properties.
    /// This is very important when the parameter itself hints at
    /// different types of serialization.  Some examples are:
    /// 
    /// ENCODING=BASE64
    /// VALUE=DATE
    /// 
    /// That is why the AssociatedCalendarParameterList was created.
    /// This class associates a CalendarDataType-based property value 
    /// with the property itself, so it can be serialized correctly.
    /// </summary>
    public class AssociatedCalendarParameterList :
        ICalendarParameterList
    {
        #region Private Fields

        ICalendarObject m_Parent;
        ICalendarParameterListContainer m_AssociatedContainer;
        ICalendarParameterList m_Parameters;

        #endregion

        #region Protected Properties

        protected ICalendarParameterList Parameters
        {
            get
            {
                if (m_AssociatedContainer != null &&
                    m_AssociatedContainer.Parameters != null)
                    return m_AssociatedContainer.Parameters;
                else
                    return m_Parameters;
            }
        }

        #endregion

        #region Constructors

        public AssociatedCalendarParameterList(ICalendarObject parent, ICalendarParameterListContainer container)
        {
            m_Parent = parent;
            m_AssociatedContainer = container;
            m_Parameters = new CalendarParameterList(parent, true);

            m_Parameters.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarParameter>>(Parameters_ItemAdded);
            m_Parameters.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarParameter>>(Parameters_ItemRemoved);
            if (container != null)
            {
                container.Parameters.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarParameter>>(Parameters_ItemAdded);
                container.Parameters.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarParameter>>(Parameters_ItemRemoved);
            }
            ItemAdded += new EventHandler<ObjectEventArgs<ICalendarParameter>>(CalendarParameterList_ItemAdded);
            ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarParameter>>(CalendarParameterList_ItemRemoved);
        }

        public AssociatedCalendarParameterList(ICalendarParameterList list, ICalendarObject parent, ICalendarParameterListContainer container) : 
            this(parent, container)
        {
            if (list != null)
            {                
                if (m_AssociatedContainer != null &&
                    m_AssociatedContainer.Parameters != null)
                {
                    foreach (ICalendarParameter p in list)
                    {
                        if (!m_AssociatedContainer.Parameters.Contains(p))
                            m_AssociatedContainer.Parameters.Add(p);
                    }
                }
                else
                {
                    foreach (ICalendarParameter p in list)
                        m_Parameters.Add(p);
                }
            }
        }

        #endregion

        #region Event Handlers

        void Parameters_ItemRemoved(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            OnItemRemoved(e.Object);
        }

        void Parameters_ItemAdded(object sender, ObjectEventArgs<ICalendarParameter> e)
        {
            OnItemAdded(e.Object);
        }

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

        public void Add(string name, string value)
        {
            if (name != null)
            {        
                Add(new CalendarParameter(name, value));
            }
        }

        public void Add(string name, string[] values)
        {
            if (name != null)
            {
                Add(new CalendarParameter(name, values));
            }
        }
        
        public void Add(string name, IList<string> values)
        {
            if (name != null)
            {
                string[] vals = new string[values.Count];
                values.CopyTo(vals, 0);
                Add(new CalendarParameter(name, vals));
            }
        }

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
                else if (values != null)
                {
                    Add(new CalendarParameter(name, values));
                }
            }
        }

        public void Set(string name, string value)
        {
            Set(name, value != null ? new string[] { value } : null);
        }
        
        public void Set(string name, IList<string> values)
        {
            string[] vals = new string[values.Count];
            values.CopyTo(vals, 0);
            Set(name, vals);
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
        
        public IList<string> GetList(string name)
        {
            return new CalendarParameterCompositeList(this, name);
        }

        #endregion

        #region IKeyedList<ICalendarParameter,string> Members

        public event EventHandler<ObjectEventArgs<ICalendarParameter>> ItemAdded;
        public event EventHandler<ObjectEventArgs<ICalendarParameter>> ItemRemoved;

        protected void OnItemAdded(ICalendarParameter p)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<ICalendarParameter>(p));
        }

        protected void OnItemRemoved(ICalendarParameter p)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<ICalendarParameter>(p));
        }

        public bool ContainsKey(string key)
        {
            return Parameters.ContainsKey(key);
        }

        public int IndexOf(string key)
        {
            return Parameters.IndexOf(key);
        }

        public int CountOf(string key)
        {
            return Parameters.CountOf(key);
        }

        public IList<ICalendarParameter> AllOf(string key)
        {
            return Parameters.AllOf(key);
        }

        public ICalendarParameter this[string key]
        {
            get
            {
                return Parameters[key];
            }
            set
            {
                Parameters[key] = value;
            }
        }

        public bool Remove(string key)
        {
            int index = IndexOf(key);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public ICalendarParameter[] ToArray()
        {
            return Parameters.ToArray();
        }

        #endregion

        #region IList<ICalendarParameter> Members

        public int IndexOf(ICalendarParameter item)
        {
            return Parameters.IndexOf(item);
        }

        public void Insert(int index, ICalendarParameter item)
        {
            Parameters.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Parameters.RemoveAt(index);
        }

        public ICalendarParameter this[int index]
        {
            get
            {
                return Parameters[index];
            }
            set
            {
                Parameters[index] = value;
            }
        }

        #endregion

        #region ICollection<ICalendarParameter> Members

        public void Add(ICalendarParameter item)
        {
            Parameters.Add(item);
        }

        public void Clear()
        {
            Parameters.Clear();
        }

        public bool Contains(ICalendarParameter item)
        {
            return Parameters.Contains(item);
        }

        public void CopyTo(ICalendarParameter[] array, int arrayIndex)
        {
            Parameters.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return Parameters.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ICalendarParameter item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<ICalendarParameter> Members

        public IEnumerator<ICalendarParameter> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        #endregion
    }
}
