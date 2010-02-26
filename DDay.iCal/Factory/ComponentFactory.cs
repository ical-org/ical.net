using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;
using System.IO;

namespace DDay.iCal
{
    public class ComponentFactory :
        ICalendarComponentFactory
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

        #region ICalendarComponentFactory Members

        virtual public ICalendarComponent Create(string objectName)
        {
            switch (objectName.ToUpper())
            {
                case ALARM: return new Alarm();
                case EVENT: return new Event();
                case FREEBUSY: return new FreeBusy();
                case JOURNAL: return new Journal();
                case TIMEZONE: return new iCalTimeZone();
                case TODO: return new Todo();
                case DAYLIGHT:
                case STANDARD:
                    return new iCalTimeZoneInfo(objectName.ToUpper());
                default: return new CalendarComponent(objectName);
            }
        }

        #endregion

        #region Static Public Methods

        /// <summary>
        /// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the iCalendar component</param>
        /// <returns>A <see cref="ComponentBase"/> object</returns>
        static public CalendarComponent LoadFromStream(Stream s) { return LoadFromStream<ComponentSerializer>(null, s, Encoding.UTF8); }
        static public CalendarComponent LoadFromStream(TextReader tr) { return LoadFromStream<ComponentSerializer>(null, tr); }
        static public T LoadFromStream<T>(TextReader tr) where T : CalendarComponent
        {
            return LoadFromStream<T, ComponentSerializer>(tr);
        }
        static public T LoadFromStream<T>(Stream s, Encoding encoding) where T : CalendarComponent
        {
            return LoadFromStream<T, ComponentSerializer>(s, encoding);
        }
        static public T LoadFromStream<T, U>(TextReader tr)
            where T : CalendarComponent
            where U : DDay.iCal.Serialization.ISerializable
        {
            if (typeof(T) == typeof(CalendarComponent) ||
                typeof(T).IsSubclassOf(typeof(CalendarComponent)))
                return (T)(object)LoadFromStream<U>(null, tr);
            else return default(T);
        }
        static public T LoadFromStream<T, U>(Stream s, Encoding encoding)
            where T : CalendarComponent
            where U : DDay.iCal.Serialization.ISerializable
        {
            if (typeof(T) == typeof(CalendarComponent) ||
                typeof(T).IsSubclassOf(typeof(CalendarComponent)))
                return (T)(object)LoadFromStream<U>(null, s, encoding);
            else return default(T);
        }
        static public CalendarComponent LoadFromStream<T>(CalendarObject parent, TextReader tr) where T : DDay.iCal.Serialization.ISerializable
        {
            string text = tr.ReadToEnd();
            tr.Close();

            byte[] memoryBlock = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream(memoryBlock);
            return LoadFromStream<T>(parent, ms, Encoding.UTF8);
        }
        static public CalendarComponent LoadFromStream<T>(CalendarObject parent, Stream s, Encoding encoding) where T : DDay.iCal.Serialization.ISerializable
        {
            DDay.iCal.Serialization.ISerializable serializable = (DDay.iCal.Serialization.ISerializable)Activator.CreateInstance(typeof(T));
            return LoadFromStream(parent, s, encoding, serializable);
        }
        static public CalendarComponent LoadFromStream(CalendarObject parent, Stream s, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            Type iCalendarType = typeof(iCalendar);
            if (parent != null)
                iCalendarType = parent.Calendar.GetType();

            CalendarComponent component = (CalendarComponent)
                serializer.Deserialize(s, encoding, iCalendarType);

            if (parent != null)
                parent.AddChild(component);

            return component;
        }

        #endregion
    }
}
