using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;

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
        #region Static Public Methods

        #region LoadFromStream(...)

        #region LoadFromStream(Stream s) variants

        /// <summary>
        /// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
        /// </summary>
        static public ICalendarComponent LoadFromStream(Stream s)
        {
            return LoadFromStream(s, Encoding.UTF8);
        }

        #endregion

        #region LoadFromStream(Stream s, Encoding e) variants

        static public ICalendarComponent LoadFromStream(Stream stream, Encoding encoding)
        {
            return LoadFromStream(stream, encoding, new ComponentSerializer());
        }

        static public T LoadFromStream<T>(Stream stream, Encoding encoding)
            where T : ICalendarComponent
        {
            ComponentSerializer serializer = new ComponentSerializer();            
            object obj = LoadFromStream(stream, encoding, serializer);
            if (obj is T)
                return (T)obj;
            return default(T);
        }

        static public ICalendarComponent LoadFromStream(Stream stream, Encoding encoding, ISerializer serializer)
        {
            return serializer.Deserialize(stream, encoding) as ICalendarComponent;
        }

        #endregion

        #region LoadFromStream(TextReader tr) variants

        static public ICalendarComponent LoadFromStream(TextReader tr)
        {
            string text = tr.ReadToEnd();
            tr.Close();

            byte[] memoryBlock = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream(memoryBlock);
            return LoadFromStream(ms, Encoding.UTF8);
        }

        static public T LoadFromStream<T>(TextReader tr) where T : ICalendarComponent
        {
            object obj = LoadFromStream(tr);
            if (obj is T)
                return (T)obj;
            return default(T);
        }

        #endregion        

        #endregion

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
