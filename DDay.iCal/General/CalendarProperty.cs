using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents a property of the <see cref="iCalendar"/>
    /// itself, or a non-standard (X-) property of an iCalendar component,
    /// as seen with many applications, such as with Apple's iCal.
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
#if DATACONTRACT
    [DataContract(Name = "Property", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarProperty : 
        CalendarObject,
        ICalendarProperty
    {
        #region Private Fields

        private string m_Value = null;
        private ICalendarParameterList m_Parameters;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 2)]
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

        public CalendarProperty(string name, string value) : this(name)
        {
            Value = value;
        }

        public CalendarProperty(int line, int col) : base(line, col)
        {
            Initialize();
        }

        private void Initialize()
        {
            m_Parameters = new CalendarParameterList();
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is ICalendarProperty)
            {
                ICalendarProperty p = (ICalendarProperty)obj;
                return p.Name.Equals(Name) &&
                    ((p.Value == null && Value == null) ||
                    (p.Value != null && p.Value.Equals(Value)));
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                (Value != null ? Value.GetHashCode() : 0);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null)
            {
                // Copy the value
                Value = p.Value;

                // Copy parameters
                foreach (ICalendarParameter parm in p.Parameters)
                    AddChild(parm.Copy<ICalendarParameter>());
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
            AddParameter(p);
        }

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        virtual public void AddParameter(ICalendarParameter p)
        {
            Parameters.Add(p);
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

        #region IKeyedObject Members

        public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
