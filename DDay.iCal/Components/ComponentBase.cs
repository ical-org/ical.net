using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.IO;

namespace DDay.iCal.Components
{
    /// <summary>
    /// This class is used by the parsing framework as a factory class
    /// for <see cref="iCalendar"/> components.  Generally, you should
    /// not need to use this class directly.
    /// </summary>
    public class ComponentBase : iCalObject
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
                case ALARM: return new Alarm(parent); break;
                case EVENT: return new Event(parent); break;
                case FREEBUSY: return new FreeBusy(parent); break;
                case JOURNAL: return new Journal(parent); break;
                case TIMEZONE: return new DDay.iCal.Components.TimeZone(parent); break;
                case TODO: return new Todo(parent); break;
                case DAYLIGHT:
                case STANDARD:
                    return new DDay.iCal.Components.TimeZone.TimeZoneInfo(name.ToUpper(), parent); break;
                default: return new ComponentBase(parent, name); break;
            }
        }

        /// <summary>
        /// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the iCalendar component</param>
        /// <returns>A <see cref="ComponentBase"/> object</returns>
        static public ComponentBase LoadFromStream(Stream s) { return LoadFromStream(null, s); }
        static public ComponentBase LoadFromStream(TextReader tr) { return LoadFromStream(null, tr); }
        static public T LoadFromStream<T>(TextReader tr)
        {
            if (typeof(T) == typeof(ComponentBase) ||
                typeof(T).IsSubclassOf(typeof(ComponentBase)))
                return (T)(object)LoadFromStream(null, tr);
            else return default(T);
        }
        static public T LoadFromStream<T>(Stream s)
        {
            if (typeof(T) == typeof(ComponentBase) ||
                typeof(T).IsSubclassOf(typeof(ComponentBase)))
                return (T)(object)LoadFromStream(null, s);
            else return default(T);
        }
        static public ComponentBase LoadFromStream(iCalObject parent, Stream s)
        {
            TextReader tr = new StreamReader(s, Encoding.UTF8);
            return LoadFromStream(parent, tr);
        }
        static public ComponentBase LoadFromStream(iCalObject parent, TextReader tr)
        {
            // Create a lexer for our text stream
            iCalLexer lexer = new iCalLexer(tr);
            iCalParser parser = new iCalParser(lexer);

            // Determine the calendar type we'll be using when constructing
            // iCalendar components...
            if (parent != null)
                parser.iCalendarType = parent.iCalendar.GetType();
            else
            {
                parent = new iCalendar();
                parser.iCalendarType = typeof(iCalendar);
            }

            // Parse the iCalendar!
            ComponentBase comp = (ComponentBase)parser.component(parent);

            // Close our text stream
            tr.Close();

            // Return the parsed component
            return comp;
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
