using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public class AssociatedCalendarParameterList :
        ICalendarParameterList
    {
        #region Private Fields

        ICalendarObject m_Parent;
        ICalendarParameterListContainer m_AssociatedContainer;
        IKeyedList<ICalendarParameter, string> m_Parameters = new KeyedList<ICalendarParameter, string>();

        #endregion

        #region Constructors

        public AssociatedCalendarParameterList(ICalendarObject parent, ICalendarParameterListContainer container)
        {
            m_Parent = parent;
            m_AssociatedContainer = container;

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

        #region ICalendarParaeterList Members

        public void Add(string name, string value)
        {
            Add(new CalendarParameter(name, value));
        }

        public void Add(string name, string[] values)
        {
            Add(new CalendarParameter(name, values));
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
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.ContainsKey(key);
            return m_Parameters.ContainsKey(key);
        }

        public int IndexOf(string key)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.IndexOf(key);
            return m_Parameters.IndexOf(key);
        }

        public int CountOf(string key)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.CountOf(key);
            return m_Parameters.CountOf(key);
        }

        public IList<ICalendarParameter> AllOf(string key)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.AllOf(key);
            return m_Parameters.AllOf(key);
        }

        public ICalendarParameter this[string key]
        {
            get
            {
                if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                    return m_AssociatedContainer.Parameters[key];
                return m_Parameters[key];
            }
            set
            {
                if (m_AssociatedContainer != null &&
                   m_AssociatedContainer.Parameters != null)
                    m_AssociatedContainer.Parameters[key] = value;
                else
                    m_Parameters[key] = value;
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
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.ToArray();
            return m_Parameters.ToArray();
        }

        #endregion

        #region IList<ICalendarParameter> Members

        public int IndexOf(ICalendarParameter item)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.IndexOf(item);
            return m_Parameters.IndexOf(item);
        }

        public void Insert(int index, ICalendarParameter item)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                m_AssociatedContainer.Parameters.Insert(index, item);
            else
                m_Parameters.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                m_AssociatedContainer.Parameters.RemoveAt(index);
            else
                m_Parameters.RemoveAt(index);
        }

        public ICalendarParameter this[int index]
        {
            get
            {
                if (m_AssociatedContainer != null && m_AssociatedContainer.Parameters != null)
                    return m_AssociatedContainer.Parameters[index];
                return m_Parameters[index];
            }
            set
            {
                if (m_AssociatedContainer != null && m_AssociatedContainer.Parameters != null)
                    m_AssociatedContainer.Parameters[index] = value;
                else
                    m_Parameters[index] = value;
            }
        }

        #endregion

        #region ICollection<ICalendarParameter> Members

        public void Add(ICalendarParameter item)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                m_AssociatedContainer.Parameters.Add(item);
            else
                m_Parameters.Add(item);
        }

        public void Clear()
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                m_AssociatedContainer.Parameters.Clear();
            else
                m_Parameters.Clear();
        }

        public bool Contains(ICalendarParameter item)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.Contains(item);
            return m_Parameters.Contains(item);
        }

        public void CopyTo(ICalendarParameter[] array, int arrayIndex)
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                m_AssociatedContainer.Parameters.CopyTo(array, arrayIndex);
            else
                m_Parameters.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                if (m_AssociatedContainer != null &&
                    m_AssociatedContainer.Parameters != null)
                    return m_AssociatedContainer.Parameters.Count;
                return m_Parameters.Count;
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
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.GetEnumerator();
            return m_Parameters.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_AssociatedContainer != null &&
                m_AssociatedContainer.Parameters != null)
                return m_AssociatedContainer.Parameters.GetEnumerator();
            return m_Parameters.GetEnumerator();
        }

        #endregion
    }
}
