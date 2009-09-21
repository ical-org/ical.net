using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.Threading;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an iCalendar object.  To load an iCalendar object, generally a
    /// static LoadFromXXX method is used.
    /// <example>
    ///     For example, use the following code to load an iCalendar object from a URL:
    ///     <code>
    ///        iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
    ///     </code>
    /// </example>
    /// Once created, an iCalendar object can be used to gathers relevant information about
    /// events, todos, time zones, journal entries, and free/busy time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following is an example of loading an iCalendar and displaying a text-based calendar.
    /// 
    /// <code>
    /// //
    /// // The following code loads and displays an iCalendar 
    /// // with US Holidays for 2006.
    /// //
    /// iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.applegatehomecare.com/Calendars/USHolidays.ics"));
    /// 
    /// List&lt;Occurrence&gt; occurrences = iCal.GetOccurrences(
    ///     new iCalDateTime(2006, 1, 1, "US-Eastern", iCal),
    ///     new iCalDateTime(2006, 12, 31, "US-Eastern", iCal));
    /// 
    /// foreach (Occurrence o in occurrences)
    /// {
    ///     Event evt = o.Component as Event;
    ///     if (evt != null)
    ///     {
    ///         // Display the date of the event
    ///         Console.Write(o.Period.StartTime.Local.Date.ToString("MM/dd/yyyy") + " -\t");
    ///
    ///         // Display the event summary
    ///         Console.Write(evt.Summary);
    ///
    ///         // Display the time the event happens (unless it's an all-day event)
    ///         if (evt.Start.HasTime)
    ///         {
    ///             Console.Write(" (" + evt.Start.Local.ToShortTimeString() + " - " + evt.End.Local.ToShortTimeString());
    ///             if (evt.Start.TimeZoneInfo != null)
    ///                 Console.Write(" " + evt.Start.TimeZoneInfo.TimeZoneName);
    ///             Console.Write(")");
    ///         }
    ///
    ///         Console.Write(Environment.NewLine);
    ///     }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// The following example loads all active to-do items from an iCalendar:
    /// 
    /// <code>
    /// //
    /// // The following code loads and displays active todo items from an iCalendar
    /// // for January 6th, 2006.    
    /// //
    /// iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));    
    /// 
    /// iCalDateTime dt = new iCalDateTime(2006, 1, 6, "US-Eastern", iCal);
    /// foreach(Todo todo in iCal.Todos)
    /// {
    ///     if (todo.IsActive(dt))
    ///     {
    ///         // Display the todo summary
    ///         Console.WriteLine(todo.Summary);
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
#if DATACONTRACT
    [DataContract(Name = "iCalendar", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#else
    [Serializable]
#endif
    public class iCalendar : ComponentBase, IDisposable
    {
        #region Constructors

        /// <summary>
        /// To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
        /// <example>
        /// For example, use the following code to load an iCalendar object from a URL:
        /// <code>
        ///     iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// </code>
        /// </example>
        /// </summary>
        public iCalendar()
            : base(null)
        {
            this.Name = "VCALENDAR";
            UniqueComponents = new UniqueComponentList<UniqueComponent>(this);
            Events = new UniqueComponentList<Event>(this);
            FreeBusy = new List<FreeBusy>();
            Journals = new UniqueComponentList<Journal>(this);
            TimeZones = new List<iCalTimeZone>();
            Todos = new UniqueComponentList<Todo>(this);

            object[] attrs = GetType().GetCustomAttributes(typeof(ComponentBaseTypeAttribute), false);
            if (attrs.Length > 0)
            {
                foreach (ComponentBaseTypeAttribute attr in attrs)
                    m_ComponentBaseCreate = attr.Type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            }

            if (m_ComponentBaseCreate == null)
                m_ComponentBaseCreate = typeof(ComponentBase).GetMethod("Create", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Adds an <see cref="iCalObject"/>-based component to the
        /// appropriate collection.  Currently, the iCalendar component
        /// supports the following components:
        ///     <list type="bullet">        
        ///         <item><see cref="DDay.iCal.Components.Event"/></item>
        ///         <item><see cref="DDay.iCal.Components.FreeBusy"/></item>
        ///         <item><see cref="DDay.iCal.Components.Journal"/></item>
        ///         <item><see cref="DDay.iCal.Components.TimeZone"/></item>
        ///         <item><see cref="DDay.iCal.Components.Todo"/></item>
        ///     </list>
        /// </summary>
        /// <param name="child"></param>
        public override void AddChild(iCalObject child)
        {
            base.AddChild(child);
            child.Parent = this;

            if (child is UniqueComponent)
                UniqueComponents.Add((UniqueComponent)child);

            Type type = child.GetType();
            if (type == typeof(Event) || type.IsSubclassOf(typeof(Event))) Events.Add((Event)child);
            else if (type == typeof(FreeBusy) || type.IsSubclassOf(typeof(FreeBusy))) FreeBusy.Add((FreeBusy)child);
            else if (type == typeof(Journal) || type.IsSubclassOf(typeof(Journal))) Journals.Add((Journal)child);
            else if (type == typeof(iCalTimeZone) || type.IsSubclassOf(typeof(iCalTimeZone))) TimeZones.Add((iCalTimeZone)child);
            else if (type == typeof(Todo) || type.IsSubclassOf(typeof(Todo))) Todos.Add((Todo)child);
        }

        /// <summary>
        /// Removes an <see cref="iCalObject"/>-based component from all
        /// of the collections that this object may be contained in within
        /// the iCalendar.
        /// </summary>
        /// <param name="child"></param>
        public override void RemoveChild(iCalObject child)
        {
            base.RemoveChild(child);

            if (child is UniqueComponent)
                UniqueComponents.Remove((UniqueComponent)child);

            Type type = child.GetType();
            if (type == typeof(Event) || type.IsSubclassOf(typeof(Event))) Events.Remove((Event)child);
            else if (type == typeof(FreeBusy) || type.IsSubclassOf(typeof(FreeBusy))) FreeBusy.Remove((FreeBusy)child);
            else if (type == typeof(Journal) || type.IsSubclassOf(typeof(Journal))) Journals.Remove((Journal)child);
            else if (type == typeof(iCalTimeZone) || type.IsSubclassOf(typeof(iCalTimeZone))) TimeZones.Remove((iCalTimeZone)child);
            else if (type == typeof(Todo) || type.IsSubclassOf(typeof(Todo))) Todos.Remove((Todo)child);
        }

        /// <summary>
        /// Resolves each UID in the UniqueComponents list
        /// to a valid UID.  When the UIDs are updated, each
        /// UniqueComponentList that contains the UniqueComponent
        /// will be updated as well.
        /// </summary>
        public override void OnLoaded(EventArgs e)
        {
            UniqueComponents.ResolveUIDs();

            base.OnLoaded(e);
        }

        /// <summary>
        /// Creates a typed copy of the iCalendar.
        /// </summary>
        /// <returns>An iCalendar object.</returns>
        public new iCalendar Copy()
        {
            return (iCalendar)base.Copy();
        }

        #endregion

        #region Private Fields

        private UniqueComponentList<UniqueComponent> m_UniqueComponents;
        private UniqueComponentList<Event> m_Events;
        private List<FreeBusy> m_FreeBusy;
        private UniqueComponentList<Journal> m_Journal;
        private List<iCalTimeZone> m_TimeZone;
        private UniqueComponentList<Todo> m_Todo;
        private MethodInfo m_ComponentBaseCreate;

        // The buffer size used to convert streams from UTF-8 to Unicode
        private const int bufferSize = 8096;

        #endregion

        #region Public Properties

        virtual public UniqueComponentList<UniqueComponent> UniqueComponents
        {
            get { return m_UniqueComponents; }
            set { m_UniqueComponents = value; }
        }

        virtual public IEnumerable<RecurringComponent> RecurringComponents
        {
            get
            {
                foreach (UniqueComponent uc in UniqueComponents)
                {
                    if (uc is RecurringComponent)
                        yield return (RecurringComponent)uc;
                }
            }
        }

        /// <summary>
        /// A collection of <see cref="Event"/> components in the iCalendar.
        /// </summary>
        virtual public UniqueComponentList<Event> Events
        {
            get { return m_Events; }
            set { m_Events = value; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.FreeBusy"/> components in the iCalendar.
        /// </summary>
        virtual public List<FreeBusy> FreeBusy
        {
            get { return m_FreeBusy; }
            set { m_FreeBusy = value; }
        }

        /// <summary>
        /// A collection of <see cref="Journal"/> components in the iCalendar.
        /// </summary>
        virtual public UniqueComponentList<Journal> Journals
        {
            get { return m_Journal; }
            set { m_Journal = value; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.TimeZone"/> components in the iCalendar.
        /// </summary>
        virtual public List<iCalTimeZone> TimeZones
        {
            get { return m_TimeZone; }
            set { m_TimeZone = value; }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        virtual public UniqueComponentList<Todo> Todos
        {
            get { return m_Todo; }
            set { m_Todo = value; }
        }

        virtual public string Version
        {
            get
            {
                if (Properties.ContainsKey("VERSION"))
                    return ((Property)Properties["VERSION"]).Value;
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value) &&
                    Properties.ContainsKey("VERSION"))
                    Properties.Remove("VERSION");
                else Properties["VERSION"] = new Property(this, "VERSION", value);
            }
        }

        virtual public string ProductID
        {
            get
            {
                if (Properties.ContainsKey("PRODID"))
                    return ((Property)Properties["PRODID"]).Value;
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value) &&
                    Properties.ContainsKey("PRODID"))
                    Properties.Remove("PRODID");
                else Properties["PRODID"] = new Property(this, "PRODID", value);
            }
        }

        virtual public string Scale
        {
            get
            {
                if (Properties.ContainsKey("CALSCALE"))
                    return ((Property)Properties["CALSCALE"]).Value;
                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Properties["CALSCALE"] = new Property(this, "CALSCALE", value);
                }
                else
                {
                    Properties.Remove("CALSCALE");
                }
            }
        }

        virtual public string Method
        {
            get
            {
                if (Properties.ContainsKey("METHOD"))
                    return ((Property)Properties["METHOD"]).Value;
                return null;
            }
            set
            {
                // NOTE: Fixes bug #1874089 - iCalendar.Method property
                if (!string.IsNullOrEmpty(value))
                {
                    Properties["METHOD"] = new Property(this, "METHOD", value);
                }
                else
                {
                    Properties.Remove("METHOD");
                }
            }
        }

        virtual public RecurrenceRestrictionType RecurrenceRestriction
        {
            get
            {
                if (Properties.ContainsKey("X-DDAY-ICAL-RECURRENCE-RESTRICTION"))
                    return
                        (RecurrenceRestrictionType)Enum.Parse(
                            typeof(RecurrenceRestrictionType),
                            ((Property)Properties["X-DDAY-ICAL-RECURRENCE-RESTRICTION"]).Value,
                            true
                        );
                return RecurrenceRestrictionType.Default;
            }
            set
            {
                Properties["X-DDAY-ICAL-RECURRENCE-RESTRICTION"] = new Property(this, "X-DDAY-ICAL-RECURRENCE-RESTRICTION", value.ToString());
            }
        }

        virtual public RecurrenceEvaluationModeType RecurrenceEvaluationMode
        {
            get
            {
                if (Properties.ContainsKey("X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE"))
                    return
                        (RecurrenceEvaluationModeType)Enum.Parse(
                            typeof(RecurrenceEvaluationModeType),
                            ((Property)Properties["X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE"]).Value,
                            true
                        );
                return RecurrenceEvaluationModeType.Default;
            }
            set
            {
                Properties["X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE"] = new Property(this, "X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE", value.ToString());
            }
        }

        #endregion

        #region Static Public Methods

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from the file system.
        /// </summary>
        /// <param name="Filepath">The path to the file to load.</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromFile(string Filepath)
        {
            return LoadFromFile(
                typeof(iCalendar),
                Filepath,
                Encoding.UTF8,
                new iCalendarSerializer());
        }
        static public T LoadFromFile<T>(string Filepath)
        {
            if (typeof(T) == typeof(iCalendar) ||
                typeof(T).IsSubclassOf(typeof(iCalendar)))
            {
                object obj = LoadFromFile(
                    typeof(T),
                    Filepath,
                    Encoding.UTF8,
                    new iCalendarSerializer());
                return (T)obj;
            }
            else return default(T);
        }
        static public iCalendar LoadFromFile(Type iCalendarType, string Filepath)
        {
            return LoadFromFile(
                iCalendarType, Filepath, Encoding.UTF8, new iCalendarSerializer());
        }
        static public iCalendar LoadFromFile(Type iCalendarType, string Filepath, Encoding encoding)
        {
            return LoadFromFile(
                iCalendarType, Filepath, encoding, new iCalendarSerializer());
        }
        static public iCalendar LoadFromFile(Type iCalendarType, string Filepath, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            FileStream fs = new FileStream(Filepath, FileMode.Open);

            iCalendar iCal = LoadFromStream(iCalendarType, fs, encoding, serializer);
            fs.Close();
            return iCal;
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static new public iCalendar LoadFromStream(Stream s)
        {
            return LoadFromStream(typeof(iCalendar), s, Encoding.UTF8);
        }
        static public iCalendar LoadFromStream(Stream s, Encoding encoding)
        {
            return LoadFromStream(typeof(iCalendar), s, encoding);
        }
        static new public iCalendar LoadFromStream(TextReader tr)
        {
            return LoadFromStream(typeof(iCalendar), tr);
        }
        static new public T LoadFromStream<T>(TextReader tr)
        {
            return LoadFromStream<T>(tr, new iCalendarSerializer());
        }
        static public T LoadFromStream<T>(TextReader tr, DDay.iCal.Serialization.ISerializable serializer)
        {
            if (typeof(T) == typeof(iCalendar) ||
                typeof(T).IsSubclassOf(typeof(iCalendar)))
                return (T)(object)LoadFromStream(typeof(T), tr, serializer);
            else return default(T);
        }
        static public T LoadFromStream<T>(Stream s)
        {
            return LoadFromStream<T>(s, Encoding.UTF8, new iCalendarSerializer());
        }
        static new public T LoadFromStream<T>(Stream s, Encoding encoding)
        {
            return LoadFromStream<T>(s, encoding, new iCalendarSerializer());
        }
        static public T LoadFromStream<T>(Stream s, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            if (typeof(T) == typeof(iCalendar) ||
                typeof(T).IsSubclassOf(typeof(iCalendar)))
                return (T)(object)LoadFromStream(typeof(T), s, encoding, serializer);
            else return default(T);
        }
        static public iCalendar LoadFromStream(Type iCalendarType, Stream s)
        {
            return LoadFromStream(iCalendarType, s, Encoding.UTF8);
        }
        static public iCalendar LoadFromStream(Type iCalendarType, Stream s, Encoding e)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            return (iCalendar)serializer.Deserialize(s, e, iCalendarType);
        }
        static public iCalendar LoadFromStream(Type iCalendarType, TextReader tr)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            return (iCalendar)serializer.Deserialize(tr, iCalendarType);
        }
        static public iCalendar LoadFromStream(Type iCalendarType, Stream s, Encoding e, DDay.iCal.Serialization.ISerializable serializer)
        {
            return (iCalendar)serializer.Deserialize(s, e, iCalendarType);
        }
        static public iCalendar LoadFromStream(Type iCalendarType, TextReader tr, DDay.iCal.Serialization.ISerializable serializer)
        {
            string text = tr.ReadToEnd();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return LoadFromStream(iCalendarType, ms, Encoding.UTF8, serializer);
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromUri(Uri uri) { return LoadFromUri(typeof(iCalendar), uri, null, null, null); }

#if !SILVERLIGHT
        static public iCalendar LoadFromUri(Uri uri, WebProxy proxy) { return LoadFromUri(typeof(iCalendar), uri, null, null, proxy); }
#endif

        static public T LoadFromUri<T>(Uri uri)
        {
            if (typeof(T) == typeof(iCalendar) ||
                typeof(T).IsSubclassOf(typeof(iCalendar)))
            {
                object obj = LoadFromUri(typeof(T), uri, null, null, null);
                return (T)obj;
            }
            else return default(T);
        }
        static public iCalendar LoadFromUri(Type iCalendarType, Uri uri)
        {
            return LoadFromUri(iCalendarType, uri, null, null, null);
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri, using a 
        /// specified <paramref name="username"/> and <paramref name="password"/>
        /// for credentials.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>an <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromUri(Uri uri, string username, string password) { return LoadFromUri(typeof(iCalendar), uri, username, password, null); }
        static public T LoadFromUri<T>(Uri uri, string username, string password)
        {
            if (typeof(T) == typeof(iCalendar) ||
                typeof(T).IsSubclassOf(typeof(iCalendar)))
            {
                object obj = LoadFromUri(typeof(T), uri, username, password, null);
                return (T)obj;
            }
            else return default(T);
        }

#if !SILVERLIGHT
        static public iCalendar LoadFromUri(Uri uri, string username, string password, WebProxy proxy) { return LoadFromUri(typeof(iCalendar), uri, username, password, proxy); }
#endif

        static public iCalendar LoadFromUri(Type iCalendarType, Uri uri, string username, string password) { return LoadFromUri(iCalendarType, uri, username, password, null); }

#if SILVERLIGHT
        static public iCalendar LoadFromUri(Type iCalendarType, Uri uri, string username, string password, object unusedProxy)
#else
        static public iCalendar LoadFromUri(Type iCalendarType, Uri uri, string username, string password, WebProxy proxy)
#endif
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);                

                if (username != null && password != null)
                    request.Credentials = new System.Net.NetworkCredential(username, password);

#if !SILVERLIGHT
                if (proxy != null)
                    request.Proxy = proxy;
#endif

                AutoResetEvent evt = new AutoResetEvent(false);

                string str = null;
                request.BeginGetResponse(new AsyncCallback(
                    delegate(IAsyncResult result)
                    {
                        Encoding e = Encoding.UTF8;

                        try
                        {
                            using (WebResponse resp = request.EndGetResponse(result))
                            {
                                // Try to determine the content encoding
                                try
                                {
                                    List<string> keys = new List<string>(resp.Headers.AllKeys);
                                    if (keys.Contains("Content-Encoding"))
                                        e = Encoding.GetEncoding(resp.Headers["Content-Encoding"]);
                                }
                                catch
                                {
                                    // Fail gracefully back to UTF-8
                                }

                                using (Stream stream = resp.GetResponseStream())
                                using (StreamReader sr = new StreamReader(stream, e))
                                {
                                    str = sr.ReadToEnd();
                                }
                            }
                        }
                        finally
                        {
                            evt.Set();
                        }
                    }
                ), null);

                evt.WaitOne();

                if (str != null)
                    return LoadFromStream(new StringReader(str));
                return null;
            }
            catch (System.Net.WebException ex)
            {
                return null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>        
        /// <returns>The time zone added to the calendar.</returns>
        public iCalTimeZone AddTimeZone(iCalTimeZone tz)
        {
            AddChild(tz);
            return tz;
        }

#if DATACONTRACT && !SILVERLIGHT
        /// <summary>
        /// Adds a system time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>
        /// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
        /// <returns>The time zone added to the calendar.</returns>
        public iCalTimeZone AddTimeZone(System.TimeZoneInfo tzi)
        {
            iCalTimeZone tz = iCalTimeZone.FromSystemTimeZone(tzi);
            AddChild(tz);
            return tz;
        }

        /// <summary>
        /// Adds the local system time zone to the iCalendar.  
        /// This time zone may then be used in date/time
        /// objects contained in the calendar.
        /// </summary>
        /// <returns>The time zone added to the calendar.</returns>
        public iCalTimeZone AddLocalTimeZone()
        {
            iCalTimeZone tz = iCalTimeZone.FromLocalTimeZone();
            AddChild(tz);
            return tz;
        }
#endif

        /// <summary>
        /// Retrieves the <see cref="DDay.iCal.Components.TimeZone" /> object for the specified
        /// <see cref="TZID"/> (Time Zone Identifier).
        /// </summary>
        /// <param name="tzid">A valid <see cref="TZID"/> object, or a valid <see cref="TZID"/> string.</param>
        /// <returns>A <see cref="TimeZone"/> object for the <see cref="TZID"/>.</returns>
        public iCalTimeZone GetTimeZone(TZID tzid)
        {
            foreach (iCalTimeZone tz in TimeZones)
            {
                if (tz.TZID.Equals(tzid))
                {
                    return tz;
                }
            }
            return null;
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time.
        /// <example>
        ///     For example, if you are displaying a month-view for January 2007,
        ///     you would want to evaluate recurrences for Jan. 1, 2007 to Jan. 31, 2007
        ///     to display relevant information for those dates.
        /// </example>
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time, for
        /// the type of recurring component specified.
        /// </summary>
        /// <typeparam name="T">The type of component to be evaluated for recurrences.</typeparam>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>
        [Obsolete("This method is no longer supported.  Use GetOccurrences() instead.")]
        public void Evaluate<T>(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        public void ClearEvaluation()
        {
            foreach (RecurringComponent rc in RecurringComponents)
                rc.ClearEvaluation();
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        public List<Occurrence> GetOccurrences(iCalDateTime dt)
        {
            return GetOccurrences<RecurringComponent>(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range.</param>
        /// <param name="ToDate">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        public List<Occurrence> GetOccurrences(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            return GetOccurrences<RecurringComponent>(FromDate, ToDate);
        }

        /// <summary>
        /// Returns all occurrences of components of type T that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime dt) where T : RecurringComponent
        {
            return GetOccurrences<T>(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        virtual public List<Occurrence> GetOccurrences<T>(iCalDateTime startTime, iCalDateTime endTime) where T : RecurringComponent
        {
            List<Occurrence> occurrences = new List<Occurrence>();
            foreach (RecurringComponent rc in RecurringComponents)
            {
                if (rc is T)
                    occurrences.AddRange(rc.GetOccurrences(startTime, endTime));
            }

            occurrences.Sort();
            return occurrences;
        }

        /// <summary>
        /// Merges the current <see cref="iCalendar"/> with another iCalendar.
        /// <note>
        ///     Since each object is associated with one and only one iCalendar object,
        ///     the <paramref name="iCal"/> that is passed is automatically Disposed
        ///     in the process, because all of its objects are re-assocated with the new iCalendar.
        /// </note>
        /// </summary>
        /// <param name="iCal">The iCalendar to merge with the current <see cref="iCalendar"/></param>
        public void MergeWith(iCalendar iCal)
        {
            if (iCal != null)
            {
                // Merge all parameters
                foreach (Parameter p in iCal.Parameters)
                {
                    if (!Parameters.ContainsKey(p.Key))
                        AddParameter(p);
                }

                // Merge all properties
                foreach (Property p in iCal.Properties)
                {
                    if (!Properties.ContainsKey(p.Key))
                        AddProperty(p);
                }

                // Merge all unique components
                foreach (UniqueComponent uc in iCal.UniqueComponents)
                {
                    if (!this.UniqueComponents.ContainsKey(uc.UID))
                        this.AddChild(uc);
                }

                // Add all time zones
                foreach (iCalTimeZone tz in iCal.TimeZones)
                {
                    // Only add the time zone if it doesn't already exist
                    if (this.GetTimeZone(tz.TZID) == null)
                        this.AddChild(tz);
                }

                // Dispose of the calendar, since we just siphoned the components from it.
                iCal.Dispose();
            }
        }

        /// <summary>
        /// Invokes the object creation of the selected ComponentBase class.  By default,
        /// this is the ComponentBase class itself; however, this can be changed to allow
        /// for custom objects to be created in lieu of Event, Todo, Journal, etc. components.
        /// <note>
        /// This method is used internally by the parsing engine to create iCalendar objects.
        /// </note>
        /// </summary>
        /// <param name="parent">The parent of the object to create.</param>
        /// <param name="name">The name of the iCal object.</param>
        /// <returns>A newly created object</returns>
        internal new ComponentBase Create(iCalObject parent, string name)
        {
            if (m_ComponentBaseCreate == null)
                throw new ArgumentException("Create() cannot be called without a valid ComponentBase Create() method attached");
            else return (ComponentBase)m_ComponentBaseCreate.Invoke(null, new object[] { parent, name });
        }

        /// <summary>
        /// Creates a typed object that is a direct child of the iCalendar itself.  Generally,
        /// you would invoke this method to create an Event, Todo, Journal, TimeZone, FreeBusy,
        /// or other base component type.
        /// </summary>
        /// <example>
        /// To create an event, use the following:
        /// <code>
        /// iCalendar iCal = new iCalendar();
        /// 
        /// Event evt = iCal.Create&lt;Event&gt;();
        /// </code>
        /// 
        /// This creates the event, and adds it to the Events list of the iCalendar.
        /// </example>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <returns>An object of the type specified</returns>
        public T Create<T>() where T : iCalObject
        {
            if (m_ComponentBaseCreate == null)
                throw new ArgumentException("Create() cannot be called without a valid ComponentBase Create() method attached");

            ConstructorInfo ci = typeof(T).GetConstructor(new Type[] { typeof(iCalObject) });
            if (ci != null)
            {
                // Create a dummy object with a null parent
                iCalObject ico = ci.Invoke(new object[] { null }) as iCalObject;
                if (ico != null)
                {
                    iCalObject resultObject = m_ComponentBaseCreate.Invoke(null, new object[] { this, ico.Name }) as iCalObject;
                    resultObject.CreateInitialize();
                    return (T)(object)resultObject;
                }
            }
            return default(T);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Children.Clear();
            Events.Clear();
            FreeBusy.Clear();
            Journals.Clear();
            Todos.Clear();
            TimeZones.Clear();
            UniqueComponents.Clear();
        }

        #endregion
    }
}
