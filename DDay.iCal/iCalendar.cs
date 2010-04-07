using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;

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
    /// IList&lt;Occurrence&gt; occurrences = iCal.GetOccurrences(
    ///     new iCalDateTime(2006, 1, 1, "US-Eastern", iCal),
    ///     new iCalDateTime(2006, 12, 31, "US-Eastern", iCal));
    /// 
    /// foreach (Occurrence o in occurrences)
    /// {
    ///     IEvent evt = o.Component as IEvent;
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

        #region LoadFromFile(...)

        #region LoadFromFile(string filepath) variants

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from the file system.
        /// </summary>
        /// <param name="Filepath">The path to the file to load.</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>        
        static public IICalendarCollection LoadFromFile(string filepath)
        {
            return LoadFromFile(filepath, Encoding.UTF8, new iCalendarSerializer());
        }

        static public IICalendarCollection LoadFromFile<T>(string filepath) where T : IICalendar
        {
            return LoadFromFile(typeof(T), filepath);
        }

        static public IICalendarCollection LoadFromFile(Type iCalendarType, string filepath)
        {
            ISerializer serializer = new iCalendarSerializer();
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalendarType;
            return LoadFromFile(filepath, Encoding.UTF8, serializer);
        }

        #endregion

        #region LoadFromFile(string filepath, Encoding encoding) variants

        static public IICalendarCollection LoadFromFile(string filepath, Encoding encoding)
        {
            return LoadFromFile(filepath, encoding, new iCalendarSerializer());
        }

        static public IICalendarCollection LoadFromFile<T>(string filepath, Encoding encoding) where T : IICalendar
        {
            return LoadFromFile(typeof(T), filepath, encoding);
        }

        static public IICalendarCollection LoadFromFile(Type iCalendarType, string filepath, Encoding encoding)
        {
            ISerializer serializer = new iCalendarSerializer();
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalendarType;
            return LoadFromFile(filepath, encoding, serializer);
        }

        static public IICalendarCollection LoadFromFile(string filepath, Encoding encoding, ISerializer serializer)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open);

            IICalendarCollection calendars = LoadFromStream(fs, encoding, serializer);
            fs.Close();
            return calendars;
        }

        #endregion

        #endregion

        #region LoadFromStream(...)

        #region LoadFromStream(Stream s) variants

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public IICalendarCollection LoadFromStream(Stream s)
        {
            return LoadFromStream(s, Encoding.UTF8, new iCalendarSerializer());
        }

        static public IICalendarCollection LoadFromStream<T>(Stream s) where T : IICalendar
        {
            return LoadFromStream(typeof(T), s);
        }

        static public IICalendarCollection LoadFromStream(Type iCalendarType, Stream s)
        {
            ISerializer serializer = new iCalendarSerializer();
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalendarType;
            return LoadFromStream(s, Encoding.UTF8, serializer);
        }

        #endregion

        #region LoadFromStream(Stream s, Encoding encoding) variants

        static public IICalendarCollection LoadFromStream(Stream s, Encoding encoding)
        {
            return LoadFromStream(s, encoding, new iCalendarSerializer());
        }

        static public IICalendarCollection LoadFromStream<T>(Stream s, Encoding encoding) where T : IICalendar
        {
            return LoadFromStream(typeof(T), s, encoding);
        }

        static public IICalendarCollection LoadFromStream(Type iCalendarType, Stream s, Encoding encoding)
        {
            ISerializer serializer = new iCalendarSerializer();
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalendarType;
            return LoadFromStream(s, encoding, serializer);
        }

        static public IICalendarCollection LoadFromStream(Stream s, Encoding e, ISerializer serializer)
        {
            return serializer.Deserialize(s, e) as IICalendarCollection;
        }

        #endregion

        #region LoadFromStream(TextReader tr) variants

        static public IICalendarCollection LoadFromStream(TextReader tr)
        {
            return LoadFromStream(tr, new iCalendarSerializer());
        }

        static public IICalendarCollection LoadFromStream<T>(TextReader tr) where T : IICalendar
        {            
            return LoadFromStream(typeof(T), tr);
        }

        static public IICalendarCollection LoadFromStream(Type iCalendarType, TextReader tr)
        {
            ISerializer serializer = new iCalendarSerializer();
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalendarType;
            return LoadFromStream(tr, serializer);
        }

        static public IICalendarCollection LoadFromStream(TextReader tr, ISerializer serializer)
        {
            string text = tr.ReadToEnd();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return LoadFromStream(ms, Encoding.UTF8, serializer);
        }

        #endregion

        #endregion

        #region LoadFromUri(...)

        #region LoadFromUri(Uri uri) variants

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public IICalendarCollection LoadFromUri(Uri uri)
        {
            return LoadFromUri(typeof(iCalendar), uri, null, null, null);
        }

        static public IICalendarCollection LoadFromUri<T>(Uri uri) where T : IICalendar
        {            
            return LoadFromUri(typeof(T), uri, null, null, null);            
        }

        static public IICalendarCollection LoadFromUri(Type iCalendarType, Uri uri)
        {
            return LoadFromUri(iCalendarType, uri, null, null, null);
        }

#if !SILVERLIGHT
        static public IICalendarCollection LoadFromUri(Uri uri, WebProxy proxy)
        {
            return LoadFromUri(typeof(iCalendar), uri, null, null, proxy);
        }

        static public IICalendarCollection LoadFromUri<T>(Uri uri, WebProxy proxy)
        {
            return LoadFromUri(typeof(T), uri, null, null, proxy);
        }

        static public IICalendarCollection LoadFromUri(Type iCalendarType, Uri uri, WebProxy proxy)
        {
            return LoadFromUri(iCalendarType, uri, null, null, proxy);
        }
#endif

        #endregion

        #region LoadFromUri(Uri uri, string username, string password) variants

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri, using a 
        /// specified <paramref name="username"/> and <paramref name="password"/>
        /// for credentials.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>an <see cref="iCalendar"/> object</returns>
        static public IICalendarCollection LoadFromUri(Uri uri, string username, string password)
        {
            return LoadFromUri(typeof(iCalendar), uri, username, password, null);
        }

        static public IICalendarCollection LoadFromUri<T>(Uri uri, string username, string password) where T : IICalendar
        {
            return LoadFromUri(typeof(T), uri, username, password, null);
        }

        static public IICalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password)
        {
            return LoadFromUri(iCalendarType, uri, username, password, null);
        }

#if !SILVERLIGHT
        static public IICalendarCollection LoadFromUri(Uri uri, string username, string password, WebProxy proxy)
        {
            return LoadFromUri(typeof(iCalendar), uri, username, password, proxy);
        }
#endif

        #endregion

        #region LoadFromUri(Type iCalendarType, Uri uri, string username, string password, WebProxy proxy)

#if SILVERLIGHT
        static public IICalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password, object unusedProxy)
#else
        static public IICalendarCollection LoadFromUri(Type iCalendarType, Uri uri, string username, string password, WebProxy proxy)
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
            catch (System.Net.WebException)
            {
                return null;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Private Fields

        private IUniqueComponentList<IUniqueComponent> m_UniqueComponents;
        private IUniqueComponentList<IEvent> m_Events;
        private IUniqueComponentList<ITodo> m_Todos;
        private IUniqueComponentList<IJournal> m_Journals;
        private IUniqueComponentList<IFreeBusy> m_FreeBusy;
        private IFilteredCalendarObjectList<ITimeZone> m_TimeZones;

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
        public iCalendar()
        {
            Initialize();
            EnsureProperties();
        }

        private void EnsureProperties()
        {
            if (string.IsNullOrEmpty(Version))
                Version = CalendarVersions.v2_0;
            if (string.IsNullOrEmpty(ProductID))
                ProductID = CalendarProductIDs.Default;
        }

        private void Initialize()
        {
            this.Name = Components.CALENDAR;            

            m_UniqueComponents = new UniqueComponentList<IUniqueComponent>(this);
            m_Events = new UniqueComponentList<IEvent>(this);
            m_Todos = new UniqueComponentList<ITodo>(this);
            m_Journals = new UniqueComponentList<IJournal>(this);
            m_FreeBusy = new UniqueComponentList<IFreeBusy>(this);
            m_TimeZones = new FilteredCalendarObjectList<ITimeZone>(this);            
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            EnsureProperties();
        }

        public override bool Equals(object obj)
        {
            IICalendar iCal = obj as iCalendar;
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
                    if (UniqueComponents.Count != iCal.UniqueComponents.Count)
                        return false;

                    IEnumerator<IUniqueComponent> e1 = UniqueComponents.GetEnumerator();
                    IEnumerator<IUniqueComponent> e2 = iCal.UniqueComponents.GetEnumerator();
                    while (e1.MoveNext())
                    {
                        if (!e2.MoveNext())
                            return false;
                        if (!object.Equals(e1.Current, e2.Current))
                            return false;
                    }
                    return !e2.MoveNext();                    
                }
                return false;
            }
            return base.Equals(obj);
        }

        #endregion        

        #region IICalendar Members

        virtual public IUniqueComponentList<IUniqueComponent> UniqueComponents
        {
            get { return m_UniqueComponents; }
        }

        virtual public IEnumerable<IRecurrable> RecurringItems
        {
            get
            {
                foreach (object obj in Children)
                {
                    if (obj is IRecurrable)
                        yield return (IRecurrable)obj;
                }
            }
        }

        /// <summary>
        /// A collection of <see cref="Event"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentList<IEvent> Events
        {
            get { return m_Events; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.FreeBusy"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentList<IFreeBusy> FreeBusy
        {
            get { return m_FreeBusy; }
        }

        /// <summary>
        /// A collection of <see cref="Journal"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentList<IJournal> Journals
        {
            get { return m_Journals; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.TimeZone"/> components in the iCalendar.
        /// </summary>
        virtual public IFilteredCalendarObjectList<ITimeZone> TimeZones
        {
            get { return m_TimeZones; }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        virtual public IUniqueComponentList<ITodo> Todos
        {
            get { return m_Todos; }
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
        public ITimeZone GetTimeZone(string tzid)
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
            foreach (IRecurrable recurrable in RecurringItems)
                recurrable.ClearEvaluation();
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// for the date provided (<paramref name="dt"/>).
        /// </summary>
        /// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
        /// <returns>A list of occurrences that occur on the given date (<paramref name="dt"/>).</returns>
        public IList<Occurrence> GetOccurrences(IDateTime dt)
        {
            return GetOccurrences<IRecurringComponent>(
                new iCalDateTime(dt.Local.Date), 
                new iCalDateTime(dt.Local.Date.AddDays(1).AddSeconds(-1)));
        }

        /// <summary>
        /// Returns a list of occurrences of each recurring component
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range.</param>
        /// <param name="ToDate">The end date/time of the range.</param>
        /// <returns>A list of occurrences that fall between the dates provided.</returns>
        public IList<Occurrence> GetOccurrences(IDateTime fromDate, IDateTime toDate)
        {
            return GetOccurrences<IRecurringComponent>(fromDate, toDate);
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
        virtual public IList<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
        {
            return GetOccurrences<T>(
                new iCalDateTime(dt.Local.Date), 
                new iCalDateTime(dt.Local.Date.AddDays(1).AddSeconds(-1)));
        }

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        virtual public IList<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            List<Occurrence> occurrences = new List<Occurrence>();
            foreach (IRecurrable recurrable in RecurringItems)
            {
                if (recurrable is T)
                    occurrences.AddRange(recurrable.GetOccurrences(startTime, endTime));
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
        /// IICalendar iCal = new iCalendar();
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
            ICalendarObject obj = Activator.CreateInstance(typeof(T)) as ICalendarObject;
            if (obj is T)
            {
                AddChild(obj);
                return (T)obj;
            }
            return default(T);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Children.Clear();
        }

        #endregion        
    }
}
