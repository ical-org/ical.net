using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.General;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers.Components;

namespace Ical.Net
{
    /// <summary>
    /// This class is used by the parsing framework for iCalendar components.
    /// Generally, you should not need to use this class directly.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
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
            var serializer = new ComponentSerializer();            
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
            var text = tr.ReadToEnd();
            tr.Close();

            var memoryBlock = Encoding.UTF8.GetBytes(text);
            var ms = new MemoryStream(memoryBlock);
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
            m_Properties = new CalendarPropertyList(this, true);
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

            var c = obj as ICalendarComponent;
            if (c != null)
            {
                Properties.Clear();
                foreach (var p in c.Properties)
                    Properties.Add(p.Copy<ICalendarProperty>());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(string name, string value)
        {
            var p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(ICalendarProperty p)
        {
            p.Parent = this;
            Properties.Set(p.Name, p);
        }

        #endregion        
    }
}
