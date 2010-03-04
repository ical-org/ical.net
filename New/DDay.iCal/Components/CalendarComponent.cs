using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

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
    public class CalendarComponent :
        CalendarObject,
        ICalendarComponent
    {
        #region Constants

        public const string ALARM = "VALARM";
        public const string EVENT = "VEVENT";
        public const string FREEBUSY = "VFREEBUSY";
        public const string TODO = "VTODO";
        public const string JOURNAL = "VJOURNAL";
        public const string TIMEZONE = "VTIMEZONE";
        public const string DAYLIGHT = "DAYLIGHT";
        public const string STANDARD = "STANDARD";

        #endregion

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
            m_Properties = new CalendarPropertyList();
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

        #region Private Methods

#if DATACONTRACT
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }
#endif

        #endregion
    }
}
