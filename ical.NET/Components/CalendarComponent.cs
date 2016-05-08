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
    [Serializable]
    [DebuggerDisplay("Component: {Name}")]
    public class CalendarComponent : CalendarObject, ICalendarComponent
    {
        /// <summary>
        /// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
        /// </summary>
        public static ICalendarComponent LoadFromStream(Stream s)
        {
            return LoadFromStream(s, Encoding.UTF8);
        }

        public static ICalendarComponent LoadFromStream(Stream stream, Encoding encoding)
        {
            return LoadFromStream(stream, encoding, new ComponentSerializer());
        }

        public static T LoadFromStream<T>(Stream stream, Encoding encoding) where T : ICalendarComponent
        {
            var serializer = new ComponentSerializer();
            object obj = LoadFromStream(stream, encoding, serializer);
            if (obj is T)
            {
                return (T) obj;
            }
            return default(T);
        }

        public static ICalendarComponent LoadFromStream(Stream stream, Encoding encoding, ISerializer serializer)
        {
            return serializer.Deserialize(stream, encoding) as ICalendarComponent;
        }

        public static ICalendarComponent LoadFromStream(TextReader tr)
        {
            var text = tr.ReadToEnd();
            tr.Close();

            var memoryBlock = Encoding.UTF8.GetBytes(text);
            var ms = new MemoryStream(memoryBlock);
            return LoadFromStream(ms, Encoding.UTF8);
        }

        public static T LoadFromStream<T>(TextReader tr) where T : ICalendarComponent
        {
            object obj = LoadFromStream(tr);
            if (obj is T)
            {
                return (T) obj;
            }
            return default(T);
        }

        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
        public virtual ICalendarPropertyList Properties { get; protected set; }

        public CalendarComponent()
        {
            Initialize();
        }

        public CalendarComponent(string name) : base(name)
        {
            Initialize();
        }

        private void Initialize()
        {
            Properties = new CalendarPropertyList(this, true);
        }

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
                {
                    Properties.Add(p.Copy<ICalendarProperty>());
                }
            }
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public virtual void AddProperty(string name, string value)
        {
            var p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public virtual void AddProperty(ICalendarProperty p)
        {
            p.Parent = this;
            Properties.Set(p.Name, p);
        }
    }
}