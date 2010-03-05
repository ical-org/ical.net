using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using DDay.iCal;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DDay.iCal
{
    /// <summary>
    /// This class is used by the parsing framework for iCalendar components.
    /// Generally, you should not need to use this class directly.
    /// </summary>
#if DATACONTRACT
    [DataContract(IsReference = true, Name = "CalendarComponent", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    [DebuggerDisplay("Component: {Name}")]
    public class CalendarComponent :
        CalendarObject,
        ICalendarComponent
    {
        #region Private Fields

        private ICalendarPropertyList m_Properties;

        #endregion

        #region ICalendarPropertyList Members

        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public ICalendarPropertyList Properties
        {
            get { return m_Properties; }
            protected set
            {
                this.m_Properties = value;
            }
        }

        #endregion

        #region Constructors

        public CalendarComponent() : base() { Initialize(); }
        public CalendarComponent(string name) : base(name) { Initialize(); }

        private void Initialize()
        {
            m_Properties = new CalendarPropertyList(this);
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(string name, string value)
        {
            CalendarProperty p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(CalendarProperty p)
        {
            p.Parent = this;
            Properties[p.Name] = p;
        }

        #endregion        
    }
}
