using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents a property of the <see cref="iCalendar"/>
    /// itself or one of its components.  It can also represent non-standard
    /// (X-) properties of an iCalendar component, as seen with many
    /// applications, such as with Apple's iCal.
    /// X-WR-CALNAME:US Holidays
    /// </summary>
    /// <remarks>
    /// Currently, the "known" properties for an iCalendar are as
    /// follows:
    /// <list type="bullet">
    ///     <item>ProdID</item>
    ///     <item>Version</item>
    ///     <item>CalScale</item>
    ///     <item>Method</item>
    /// </list>
    /// There may be other, custom X-properties applied to the calendar,
    /// and X-properties may be applied to calendar components.
    /// </remarks>
    [DebuggerDisplay("{Name}:{Value}")]
#if DATACONTRACT
    [DataContract(Name = "CalendarProperty", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarProperty : 
        CalendarObject,
        ICalendarProperty
    {
        #region Private Fields

        private object m_Value;        
        private ICalendarParameterList m_Parameters;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public ICalendarParameterList Parameters
        {
            get { return m_Parameters; }
            protected set
            {
                this.m_Parameters = value;
            }
        }

        #endregion

        #region Constructors

        public CalendarProperty() : base()
        {
            Initialize();
        }

        public CalendarProperty(ICalendarProperty other) : this()
        {
            CopyFrom(other);
        }

        public CalendarProperty(string name) : base(name)
        {
            Initialize();
        }

        public CalendarProperty(string name, object value) : base(name)
        {
            Initialize();
            m_Value = value;
        }

        public CalendarProperty(int line, int col) : base(line, col)
        {
            Initialize();
        }

        private void Initialize()
        {
            m_Parameters = new CalendarParameterList(this);
        }

        #endregion

        #region Protected Methods

        protected void AssociateItem(object item)
        {
            if (item is ICalendarDataType)
                ((ICalendarDataType)item).AssociateWith(Parent);
            else if (item is ICalendarObject)
                ((ICalendarObject)item).Parent = Parent;
        }

        protected void DeassociateItem(object item)
        {
            if (item is ICalendarDataType)
                ((ICalendarDataType)item).Deassociate();
            else if (item is ICalendarObject)
                ((ICalendarObject)item).Parent = null;
        }

        protected void AssociateList(ICompositeList list)
        {
            if (list != null)
            {
                // Associate each item in the list with the parent
                foreach (object obj in list)
                    AssociateItem(obj);

                list.ItemAdded += new EventHandler<ObjectEventArgs<object>>(list_ItemAdded);
                list.ItemRemoved += new EventHandler<ObjectEventArgs<object>>(list_ItemRemoved);
            }
        }

        protected void DeassociateList(ICompositeList list)
        {
            if (list != null)
            {
                // Deassociate each item in the list
                foreach (object obj in list)
                    DeassociateItem(obj);

                list.ItemAdded -= new EventHandler<ObjectEventArgs<object>>(list_ItemAdded);
                list.ItemRemoved -= new EventHandler<ObjectEventArgs<object>>(list_ItemRemoved);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        virtual public void AddParameter(string name, string value)
        {
            CalendarParameter p = new CalendarParameter(name, value);
            Parameters.Add(p);
        }

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        virtual public void AddParameter(ICalendarParameter p)
        {
            Parameters.Add(p);
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null)
            {               
                // Copy parameters
                foreach (ICalendarParameter parm in p.Parameters)
                    AddChild(parm.Copy<ICalendarParameter>());
            }
        }

        #endregion        

        #region Event Handlers
        
        void list_ItemAdded(object sender, ObjectEventArgs<object> e)
        {
            AssociateItem(e.Object);
        }

        void list_ItemRemoved(object sender, ObjectEventArgs<object> e)
        {
            DeassociateItem(e.Object);
        }

        #endregion

        #region ICalendarProperty Members

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected void OnValueChanged(object oldValue, object newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs(oldValue, newValue));
        }

        virtual public object Value
        {
            get { return m_Value; }
            set 
            {
                if (!object.Equals(m_Value, value))
                {
                    object old = m_Value;
                    m_Value = value;

                    // Deassociate the old value
                    if (old is ICompositeList)
                        DeassociateList((ICompositeList)old);
                    else if (old != null)
                        DeassociateItem(old);

                    // Associate the new value
                    if (m_Value is ICompositeList)
                        AssociateList((ICompositeList)m_Value);
                    else if (m_Value != null)
                        AssociateItem(m_Value);

                    // Notify that the value changed
                    OnValueChanged(old, m_Value);
                }
            }
        }

        #endregion

        #region IKeyedObject Members

        virtual public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
