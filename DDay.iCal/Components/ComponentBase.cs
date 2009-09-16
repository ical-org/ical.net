using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar.Components;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// This class is used by the parsing framework as a factory class
    /// for <see cref="iCalendar"/> components.  Generally, you should
    /// not need to use this class directly.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "ComponentBase", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#else
    [Serializable]
#endif
    public class ComponentBase :
        iCalObject
    {
        public const string ALARM = "VALARM";
        public const string EVENT = "VEVENT";
        public const string FREEBUSY = "VFREEBUSY";
        public const string TODO = "VTODO";
        public const string JOURNAL = "VJOURNAL";
        public const string TIMEZONE = "VTIMEZONE";
        public const string DAYLIGHT = "DAYLIGHT";
        public const string STANDARD = "STANDARD";        

        #region Constructors

        public ComponentBase() : base() { }
        public ComponentBase(iCalObject parent) : base(parent) { }
        public ComponentBase(iCalObject parent, string name) : base(parent, name) { }

        #endregion

        #region Static Public Methods

        static public ComponentBase Create(iCalObject parent, string name)
        {
            switch(name.ToUpper())
            {
                case ALARM: return new Alarm(parent); 
                case EVENT: return new Event(parent); 
                case FREEBUSY: return new FreeBusy(parent);
                case JOURNAL: return new Journal(parent); 
                case TIMEZONE: return new iCalTimeZone(parent); 
                case TODO: return new Todo(parent); 
                case DAYLIGHT:
                case STANDARD:
                    return new iCalTimeZoneInfo(name.ToUpper(), parent);
                default: return new ComponentBase(parent, name);
            }
        }

        /// <summary>
        /// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the iCalendar component</param>
        /// <returns>A <see cref="ComponentBase"/> object</returns>
        static public ComponentBase LoadFromStream(Stream s) { return LoadFromStream<ComponentBaseSerializer>(null, s, Encoding.UTF8); }        
        static public ComponentBase LoadFromStream(TextReader tr) { return LoadFromStream<ComponentBaseSerializer>(null, tr); }
        static public T LoadFromStream<T>(TextReader tr) where T : ComponentBase 
        {
            return LoadFromStream<T, ComponentBaseSerializer>(tr);
        }
        static public T LoadFromStream<T>(Stream s, Encoding encoding) where T : ComponentBase
        {
            return LoadFromStream<T, ComponentBaseSerializer>(s, encoding);
        }
        static public T LoadFromStream<T, U>(TextReader tr)
            where T : ComponentBase
            where U : DDay.iCal.Serialization.ISerializable
        {
            if (typeof(T) == typeof(ComponentBase) ||
                typeof(T).IsSubclassOf(typeof(ComponentBase)))
                return (T)(object)LoadFromStream<U>(null, tr);
            else return default(T);
        }
        static public T LoadFromStream<T, U>(Stream s, Encoding encoding)
            where T : ComponentBase
            where U : DDay.iCal.Serialization.ISerializable
        {
            if (typeof(T) == typeof(ComponentBase) ||
                typeof(T).IsSubclassOf(typeof(ComponentBase)))
                return (T)(object)LoadFromStream<U>(null, s, encoding);
            else return default(T);
        }
        static public ComponentBase LoadFromStream<T>(iCalObject parent, TextReader tr) where T : DDay.iCal.Serialization.ISerializable            
        {
            string text = tr.ReadToEnd();
            tr.Close();

            byte[] memoryBlock = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream(memoryBlock);
            return LoadFromStream<T>(parent, ms, Encoding.UTF8);
        }
        static public ComponentBase LoadFromStream<T>(iCalObject parent, Stream s, Encoding encoding) where T : DDay.iCal.Serialization.ISerializable
        {
            DDay.iCal.Serialization.ISerializable serializable = (DDay.iCal.Serialization.ISerializable)Activator.CreateInstance(typeof(T));
            return LoadFromStream(parent, s, encoding, serializable);
        }
        static public ComponentBase LoadFromStream(iCalObject parent, Stream s, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            Type iCalendarType = typeof(iCalendar);
            if (parent != null)
                iCalendarType = parent.iCalendar.GetType();

            ComponentBase component = (ComponentBase)
                serializer.Deserialize(s, encoding, iCalendarType);

            if (parent != null)
                parent.AddChild(component);

            return component;
        }        

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(string name, string value)
        {
            Property p = new Property(this, name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(Property p)
        {
            Properties[p.Name] = p;
        }

        #endregion

        #region Overrides

        protected override System.Collections.Generic.List<object> SerializedItems
        {
            get
            {
                //
                // Get a list of all the public fields and properties
                // that are marked as Serialized.
                //
                List<object> List = new List<object>();
                foreach (FieldInfo fi in GetType().GetFields())
                    if (fi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(fi);
                foreach (PropertyInfo pi in GetType().GetProperties())
                    if (pi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(pi);
                return List;
            }
        }

        #endregion
    }
}
