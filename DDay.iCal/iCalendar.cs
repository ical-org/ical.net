using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an iCalendar object.  To load an iCalendar object, generally a
    /// static LoadFromXXX method is used.
    /// <example>
    ///     For example, use the following code to load an iCalendar object from a URL:
    ///     <code>
    ///        IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
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
    /// IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.applegatehomecare.com/Calendars/USHolidays.ics"));
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
    /// IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));    
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
    [DataContract(IsReference = true, Name = "iCalendar", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
    [KnownType(typeof(UniqueComponentList<IUniqueComponent>))]
    [KnownType(typeof(UniqueComponentList<IEvent>))]
    [KnownType(typeof(UniqueComponentList<ITodo>))]
    [KnownType(typeof(UniqueComponentList<IJournal>))]
    //[KnownType(typeof(Event))]
    //[KnownType(typeof(Todo))]
    //[KnownType(typeof(Journal))]
    //[KnownType(typeof(FreeBusy))]
#endif
    [Serializable]
    public class iCalendar : 
        CalendarComponent,
        IICalendar,
        IDisposable
    {
        #region Static Public Methods

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from the file system.
        /// </summary>
        /// <param name="Filepath">The path to the file to load.</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendarCollection LoadFromFile(string Filepath)
        {
            return LoadFromFile(
                typeof(iCalendar),
                Filepath,
                Encoding.UTF8,
                new iCalendarSerializer());
        }
        static public iCalendarCollection LoadFromFile<T>(string Filepath)
        {
            if (typeof(iCalendar).IsAssignableFrom(typeof(T)))
            {
                iCalendarCollection calendars = LoadFromFile(
                    typeof(T),
                    Filepath,
                    Encoding.UTF8,
                    new iCalendarSerializer());
                return calendars;
            }
            else return null;
        }
        static public iCalendarCollection LoadFromFile(Type iCalendarType, string Filepath)
        {
            return LoadFromFile(
                iCalendarType, Filepath, Encoding.UTF8, new iCalendarSerializer());
        }
        static public iCalendarCollection LoadFromFile(Type iCalendarType, string Filepath, Encoding encoding)
        {
            return LoadFromFile(
                iCalendarType, Filepath, encoding, new iCalendarSerializer());
        }
        static public iCalendarCollection LoadFromFile(Type iCalendarType, string Filepath, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            FileStream fs = new FileStream(Filepath, FileMode.Open);

            iCalendarCollection calendars = LoadFromStream(iCalendarType, fs, encoding, serializer);
            fs.Close();
            return calendars;
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static new public iCalendarCollection LoadFromStream(Stream s)
        {
            return LoadFromStream(typeof(iCalendar), s, Encoding.UTF8);
        }
        static public iCalendarCollection LoadFromStream(Stream s, Encoding encoding)
        {
            return LoadFromStream(typeof(iCalendar), s, encoding);
        }
        static new public iCalendarCollection LoadFromStream(TextReader tr)
        {
            return LoadFromStream(typeof(iCalendar), tr);
        }
        static new public iCalendarCollection LoadFromStream<T>(TextReader tr)
        {
            return LoadFromStream<T>(tr, new iCalendarSerializer());
        }
        static public iCalendarCollection LoadFromStream<T>(TextReader tr, DDay.iCal.Serialization.ISerializable serializer)
        {
            if (typeof(iCalendar).IsAssignableFrom(typeof(T)))
                return LoadFromStream(typeof(T), tr, serializer);
            else return null;
        }
        static public iCalendarCollection LoadFromStream<T>(Stream s)
        {
            return LoadFromStream<T>(s, Encoding.UTF8, new iCalendarSerializer());
        }
        static new public iCalendarCollection LoadFromStream<T>(Stream s, Encoding encoding)
        {
            return LoadFromStream<T>(s, encoding, new iCalendarSerializer());
        }
        static public iCalendarCollection LoadFromStream<T>(Stream s, Encoding encoding, DDay.iCal.Serialization.ISerializable serializer)
        {
            if (typeof(iCalendar).IsAssignableFrom(typeof(T)))
                return LoadFromStream(typeof(T), s, encoding, serializer);
            else return null;
        }
        static public iCalendarCollection LoadFromStream(Type iCalendarType, Stream s)
        {
            return LoadFromStream(iCalendarType, s, Encoding.UTF8);
        }
        static public iCalendarCollection LoadFromStream(Type iCalendarType, Stream s, Encoding e)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            return (iCalendarCollection)serializer.Deserialize(s, e, iCalendarType);
        }
        static public iCalendarCollection LoadFromStream(Type iCalendarType, TextReader tr)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            return (iCalendarCollection)serializer.Deserialize(tr, iCalendarType);
        }
        static public iCalendarCollection LoadFromStream(Type iCalendarType, Stream s, Encoding e, DDay.iCal.Serialization.ISerializable serializer)
        {
            return (iCalendarCollection)serializer.Deserialize(s, e, iCalendarType);
        }
        static public iCalendarCollection LoadFromStream(Type iCalendarType, TextReader tr, DDay.iCal.Serialization.ISerializable serializer)
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
        static public iCalendarCollection LoadFromUri(Uri uri) { return LoadFromUri(typeof(iCalendar), uri, null, null, null); }

#if !SILVERLIGHT
        static public iCalendarCollection LoadFromUri(Uri uri, WebProxy proxy) { return LoadFromUri(typeof(iCalendar), uri, null, null, proxy); }
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
        static public iCalendarCollection LoadFromUri(Type iCalendarType, Uri uri)
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
        static public iCalendarCollection LoadFromUri(Uri uri, string username, string password) { return LoadFromUri(typeof(iCalendar), uri, username, password, null); }
        static public iCalendarCollection LoadFromUri<T>(Uri uri, string username, string password)
        {
            if (typeof(iCalendar).IsAssignableFrom(typeof(T)))
            {
                iCalendarCollection calendars = LoadFromUri(typeof(T), uri, username, password, null);
                return calendars;
            }
            else return null;
        }

#if !SILVERLIGHT
        static public iCalendarCollection LoadFromUri(Uri uri, string username, string password, WebProxy proxy) { return LoadFromUri(typeof(iCalendar), uri, username, password, proxy); }
#endif

        static public iCalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password) { return LoadFromUri(iCalendarType, uri, username, password, null); }

#if SILVERLIGHT
        static public iCalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password, object unusedProxy)
#else
        static public iCalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password, WebProxy proxy)
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

        #region Private Fields

        private UniqueComponentList<IUniqueComponent> m_UniqueComponents;
        private UniqueComponentList<IEvent> m_Events;
        private UniqueComponentList<IFreeBusy> m_FreeBusy;
        private UniqueComponentList<IJournal> m_Journal;
        private List<ITimeZone> m_TimeZone;
        private UniqueComponentList<ITodo> m_Todo;

        [field: NonSerialized]
        private ICalendarComponentFactory m_ComponentFactory;

        // The buffer size used to convert streams from UTF-8 to Unicode
        private const int bufferSize = 8096;

        #endregion

        #region Constructors

        /// <summary>
        /// To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
        /// <example>
        /// For example, use the following code to load an iCalendar object from a URL:
        /// <code>
        ///     IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// </code>
        /// </example>
        /// </summary>
        public iCalendar() : base("VCALENDAR")
        {
            Initialize();
        }

        private void Initialize()
        {
            m_UniqueComponents = new UniqueComponentList<IUniqueComponent>();
            m_Events = new UniqueComponentList<IEvent>();
            m_FreeBusy = new UniqueComponentList<IFreeBusy>();
            m_Journal = new UniqueComponentList<IJournal>();
            m_TimeZone = new List<ITimeZone>();
            m_Todo = new UniqueComponentList<ITodo>();

            object[] attrs = GetType().GetCustomAttributes(typeof(ComponentFactoryAttribute), false);
            if (attrs.Length > 0)
            {
                foreach (ComponentFactoryAttribute attr in attrs)
                    m_ComponentFactory = Activator.CreateInstance(attr.Type) as ICalendarComponentFactory;
            }

            if (m_ComponentFactory == null)
                m_ComponentFactory = new ComponentFactory();
        }

        #endregion

        #region Private Methods

#if DATACONTRACT
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (object child in Children)
            {
                if (child is IUniqueComponent)
                    m_UniqueComponents.Add((IUniqueComponent)child);

                Type type = child.GetType();
                if (typeof(IEvent).IsAssignableFrom(type)) m_Events.Add((IEvent)child);
                else if (typeof(IFreeBusy).IsAssignableFrom(type)) m_FreeBusy.Add((IFreeBusy)child);
                else if (typeof(IJournal).IsAssignableFrom(type)) m_Journal.Add((IJournal)child);
                else if (typeof(ITimeZone).IsAssignableFrom(type)) m_TimeZone.Add((ITimeZone)child);
                else if (typeof(ITodo).IsAssignableFrom(type)) m_Todo.Add((ITodo)child);
            }
        }
#endif

        #endregion

        #region Overrides

        /// <summary>
        /// Adds an <see cref="iCalObject"/>-based component to the
        /// appropriate collection.  Currently, the iCalendar component
        /// supports the following components:
        ///     <list type="bullet">        
        ///         <item><see cref="DDay.iCal.Event"/></item>
        ///         <item><see cref="DDay.iCal.FreeBusy"/></item>
        ///         <item><see cref="DDay.iCal.Journal"/></item>
        ///         <item><see cref="DDay.iCal.TimeZone"/></item>
        ///         <item><see cref="DDay.iCal.Todo"/></item>
        ///     </list>
        /// </summary>
        /// <param name="child"></param>
        public override void AddChild(ICalendarObject child)
        {
            base.AddChild(child);
            child.Parent = this;

            if (child is IUniqueComponent)
                m_UniqueComponents.Add((IUniqueComponent)child);

            Type type = child.GetType();
            if (typeof(IEvent).IsAssignableFrom(type)) m_Events.Add((IEvent)child);
            else if (typeof(IFreeBusy).IsAssignableFrom(type)) m_FreeBusy.Add((IFreeBusy)child);
            else if (typeof(IJournal).IsAssignableFrom(type)) m_Journal.Add((IJournal)child);
            else if (typeof(ITimeZone).IsAssignableFrom(type)) m_TimeZone.Add((ITimeZone)child);
            else if (typeof(ITodo).IsAssignableFrom(type)) m_Todo.Add((ITodo)child);
        }

        /// <summary>
        /// Removes an <see cref="iCalObject"/>-based component from all
        /// of the collections that this object may be contained in within
        /// the iCalendar.
        /// </summary>
        /// <param name="child"></param>
        public override void RemoveChild(ICalendarObject child)
        {
            base.RemoveChild(child);

            if (child is IUniqueComponent)
                m_UniqueComponents.Remove((IUniqueComponent)child);

            Type type = child.GetType();
            if (typeof(IEvent).IsAssignableFrom(type)) m_Events.Remove((IEvent)child);
            else if (typeof(IFreeBusy).IsAssignableFrom(type)) m_FreeBusy.Remove((IFreeBusy)child);
            else if (typeof(IJournal).IsAssignableFrom(type)) m_Journal.Remove((IJournal)child);
            else if (typeof(ITimeZone).IsAssignableFrom(type)) m_TimeZone.Remove((ITimeZone)child);
            else if (typeof(ITodo).IsAssignableFrom(type)) m_Todo.Remove((ITodo)child);
        }

        /// <summary>
        /// Resolves each UID in the UniqueComponents list
        /// to a valid UID.  When the UIDs are updated, each
        /// UniqueComponentList that contains the UniqueComponent
        /// will be updated as well.
        /// </summary>
        public override void OnLoaded()
        {
            m_UniqueComponents.ResolveUIDs();
            base.OnLoaded();
        }

        public override bool Equals(object obj)
        {
            iCalendar iCal = obj as iCalendar;
            if (iCal != null)
            {
                bool isEqual =
                    object.Equals(Version, iCal.Version) &&
                    object.Equals(ProductID, iCal.ProductID) &&
                    object.Equals(Scale, iCal.Scale) &&
                    object.Equals(Method, iCal.Method) &&
                    (
                        (UniqueComponents == null && iCal.UniqueComponents == null) ||
                        (UniqueComponents != null && iCal.UniqueComponents != null && object.Equals(UniqueComponents.Count, iCal.UniqueComponents.Count))
                    );

                if (isEqual)
                {
                    for (int i = 0; i < UniqueComponents.Count; i++)
                    {
                        if (!object.Equals(UniqueComponents[i], iCal.UniqueComponents[i]))
                            return false;
                    }
                    return true;
                }
                return false;
            }
            return base.Equals(obj);
        }

        #endregion        

        #region IICalendar Members
                
        virtual public ICalendarComponentFactory ComponentFactory
        {
            get { return m_ComponentFactory; }
            set { m_ComponentFactory = value; }
        }

        virtual public IUniqueComponentListReadonly<IUniqueComponent> UniqueComponents
        {
            get { return m_UniqueComponents; }
        }

        virtual public IEnumerable<IRecurringComponent> RecurringComponents
        {
            get
            {
                foreach (IUniqueComponent uc in UniqueComponents)
                {
                    if (uc is IRecurringComponent)
                        yield return (IRecurringComponent)uc;
                }
            }
        }

        /// <summary>
        /// A collection of <see cref="Event"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<IEvent> Events
        {
            get { return m_Events; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.FreeBusy"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<IFreeBusy> FreeBusy
        {
            get { return m_FreeBusy; }
        }

        /// <summary>
        /// A collection of <see cref="Journal"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<IJournal> Journals
        {
            get { return m_Journal; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.TimeZone"/> components in the iCalendar.
        /// </summary>
        virtual public IList<ITimeZone> TimeZones
        {
            get { return m_TimeZone; }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentListReadonly<ITodo> Todos
        {
            get { return m_Todo; }
        }

        virtual public string Version
        {
            get { return Properties.Get<string>("VERSION"); }
            set { Properties.Set("VERSION", value); }
        }

        virtual public string ProductID
        {
            get { return Properties.Get<string>("PRODID"); }
            set { Properties.Set("PRODID", value); }
        }

        virtual public string Scale
        {
            get { return Properties.Get<string>("CALSCALE"); }
            set { Properties.Set("CALSCALE", value); }
        }

        virtual public string Method
        {
            get { return Properties.Get<string>("METHOD"); }
            set { Properties.Set("METHOD", value); }
        }

        virtual public RecurrenceRestrictionType RecurrenceRestriction
        {
            get { return Properties.Get<RecurrenceRestrictionType>("X-DDAY-ICAL-RECURRENCE-RESTRICTION"); }
            set { Properties.Set("X-DDAY-ICAL-RECURRENCE-RESTRICTION", value); }
        }

        virtual public RecurrenceEvaluationModeType RecurrenceEvaluationMode
        {
            get { return Properties.Get<RecurrenceEvaluationModeType>("X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE"); }
            set { Properties.Set("X-DDAY-ICAL-RECURRENCE-EVALUATION-MODE", value); }
        }

        /// <summary>
        /// Adds a time zone to the iCalendar.  This time zone may
        /// then be used in date/time objects contained in the 
        /// calendar.
        /// </summary>        
        /// <returns>The time zone added to the calendar.</returns>
        public ITimeZone AddTimeZone(ITimeZone tz)
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
        public ITimeZone AddTimeZone(System.TimeZoneInfo tzi)
        {
            ITimeZone tz = iCalTimeZone.FromSystemTimeZone(tzi);
            AddChild(tz);
            return tz;
        }

        /// <summary>
        /// Adds the local system time zone to the iCalendar.  
        /// This time zone may then be used in date/time
        /// objects contained in the calendar.
        /// </summary>
        /// <returns>The time zone added to the calendar.</returns>
        public ITimeZone AddLocalTimeZone()
        {
            ITimeZone tz = iCalTimeZone.FromLocalTimeZone();
            AddChild(tz);
            return tz;
        }
#endif

        /// <summary>
        /// Retrieves the <see cref="DDay.iCal.TimeZone" /> object for the specified
        /// <see cref="TZID"/> (Time Zone Identifier).
        /// </summary>
        /// <param name="tzid">A valid <see cref="TZID"/> object, or a valid <see cref="TZID"/> string.</param>
        /// <returns>A <see cref="TimeZone"/> object for the <see cref="TZID"/>.</returns>
        public ITimeZone GetTimeZone(ITZID tzid)
        {
            foreach (ITimeZone tz in TimeZones)
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
        public void Evaluate(IDateTime FromDate, IDateTime ToDate)
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
        public void Evaluate<T>(IDateTime FromDate, IDateTime ToDate)
        {
            throw new NotSupportedException("Evaluate() is no longer supported as a public method.  Use GetOccurrences() instead.");
        }

        /// <summary>
        /// Clears recurrence evaluations for recurring components.        
        /// </summary>        
        public void ClearEvaluation()
        {
            foreach (IRecurringComponent rc in RecurringComponents)
                rc.ClearEvaluation();
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        public IList<IOccurrence> GetOccurrences(IDateTime dt)
        {
            return GetOccurrences<IRecurringComponent>(dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range.</param>
        /// <param name="ToDate">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        public IList<IOccurrence> GetOccurrences(IDateTime FromDate, IDateTime ToDate)
        {
            return GetOccurrences<IRecurringComponent>(FromDate, ToDate);
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
        virtual public IList<IOccurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
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
        virtual public IList<IOccurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            List<IOccurrence> occurrences = new List<IOccurrence>();
            foreach (IRecurringComponent rc in RecurringComponents)
            {
                if (rc is T)
                    occurrences.AddRange(rc.GetOccurrences(startTime, endTime));
            }

            occurrences.Sort();
            return occurrences;
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
        public T Create<T>() where T : ICalendarComponent
        {
            if (m_ComponentFactory == null)
                throw new ArgumentException("Create() cannot be called without a valid ComponentFactory assigned to the calendar.");

            ICalendarObject obj = Activator.CreateInstance(typeof(T)) as ICalendarObject;
            if (obj is T)
            {
                AddChild(obj);

                // Initialize the object
                obj.CreateInitialize();

                return (T)obj;
            }
            return default(T);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Children.Clear();
            m_Events.Clear();
            m_FreeBusy.Clear();
            m_Journal.Clear();
            m_Todo.Clear();
            m_TimeZone.Clear();
            m_UniqueComponents.Clear();
        }

        #endregion        
    }
}
