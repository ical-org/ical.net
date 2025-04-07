//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Proxies;
using Ical.Net.Serialization;
using Ical.Net.Utility;

namespace Ical.Net;

public class Calendar : CalendarComponent, IGetOccurrencesTyped, IGetFreeBusy, IMergeable
{
    public static Calendar Load(string iCalendarString)
        => CalendarCollection.Load(new StringReader(iCalendarString)).SingleOrDefault();

    /// <summary>
    /// Loads an <see cref="Calendar"/> from an open stream.
    /// </summary>
    /// <param name="s">The stream from which to load the <see cref="Calendar"/> object</param>
    /// <returns>An <see cref="Calendar"/> object</returns>
    public static Calendar Load(Stream s)
        => CalendarCollection.Load(new StreamReader(s, Encoding.UTF8)).SingleOrDefault();

    public static Calendar Load(TextReader tr)
        => CalendarCollection.Load(tr)?.SingleOrDefault();

    public static IList<T> Load<T>(Stream s, Encoding e)
        => Load<T>(new StreamReader(s, e));

    public static IList<T> Load<T>(TextReader tr)
        => SimpleDeserializer.Default.Deserialize(tr).OfType<T>().ToList();

    public static IList<T> Load<T>(string ical)
        => Load<T>(new StringReader(ical));

    private IUniqueComponentList<IUniqueComponent> _mUniqueComponents;
    private IUniqueComponentList<CalendarEvent> _mEvents;
    private IUniqueComponentList<Todo> _mTodos;
    private ICalendarObjectList<Journal> _mJournals;
    private IUniqueComponentList<FreeBusy> _mFreeBusy;
    private ICalendarObjectList<VTimeZone> _mTimeZones;

    /// <summary>
    /// To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
    /// <example>
    /// For example, use the following code to load an iCalendar object from a URL:
    /// <code>
    ///     IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
    /// </code>
    /// </example>
    /// </summary>
    public Calendar()
    {
        // Note: ProductId and Version Property values will be empty before _deserialization_
        ProductId = LibraryMetadata.ProdId;
        Version = LibraryMetadata.Version;

        Initialize();
    }

    private void Initialize()
    {
        Name = Components.Calendar;
        _mUniqueComponents = new UniqueComponentListProxy<IUniqueComponent>(Children);
        _mEvents = new UniqueComponentListProxy<CalendarEvent>(Children);
        _mTodos = new UniqueComponentListProxy<Todo>(Children);
        _mJournals = new CalendarObjectListProxy<Journal>(Children);
        _mFreeBusy = new UniqueComponentListProxy<FreeBusy>(Children);
        _mTimeZones = new CalendarObjectListProxy<VTimeZone>(Children);
    }

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);

        Initialize();
    }

    protected bool Equals(Calendar other)
        => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
           && CollectionHelpers.Equals(UniqueComponents, other.UniqueComponents)
           && CollectionHelpers.Equals(Events, other.Events)
           && CollectionHelpers.Equals(Todos, other.Todos)
           && CollectionHelpers.Equals(Journals, other.Journals)
           && CollectionHelpers.Equals(FreeBusy, other.FreeBusy)
           && CollectionHelpers.Equals(TimeZones, other.TimeZones);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        return obj.GetType() == GetType() && Equals((Calendar) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Name?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(UniqueComponents);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Events);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Todos);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Journals);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(FreeBusy);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(TimeZones);
            return hashCode;
        }
    }

    public virtual IUniqueComponentList<IUniqueComponent> UniqueComponents => _mUniqueComponents;

    public virtual IEnumerable<IRecurrable> RecurringItems => Children.OfType<IRecurrable>();

    /// <summary>
    /// A collection of <see cref="CalendarEvent"/> components in the iCalendar.
    /// </summary>
    public virtual IUniqueComponentList<CalendarEvent> Events => _mEvents;

    /// <summary>
    /// A collection of <see cref="CalendarComponents.FreeBusy"/> components in the iCalendar.
    /// </summary>
    public virtual IUniqueComponentList<FreeBusy> FreeBusy => _mFreeBusy;

    /// <summary>
    /// A collection of <see cref="Journal"/> components in the iCalendar.
    /// </summary>
    public virtual ICalendarObjectList<Journal> Journals => _mJournals;

    /// <summary>
    /// A collection of VTimeZone components in the iCalendar.
    /// </summary>
    public virtual ICalendarObjectList<VTimeZone> TimeZones => _mTimeZones;

    /// <summary>
    /// A collection of <see cref="CalendarComponents.Todo"/> components in the iCalendar.
    /// </summary>
    public virtual IUniqueComponentList<Todo> Todos => _mTodos;

    /// <summary>
    /// Gets or sets the version of the iCalendar definition. The default is <see cref="LibraryMetadata.Version"/>
    /// as per RFC 5545 Section 3.7.4 and must be specified.
    /// <para/>
    /// It specifies the identifier corresponding to the highest version number of the iCalendar specification
    /// that is required in order to interpret the iCalendar object.
    /// <para/>
    /// <b>Do not change unless you are sure about the consequences.</b>
    /// <para/>
    /// The default value does not apply to deserialized objects.
    /// </summary>
    public virtual string Version
    {
        get => Properties.Get<string>("VERSION");
        set => Properties.Set("VERSION", value);
    }

    /// <summary>
    /// Gets or sets the product ID of the iCalendar, which typically contains the name of the software
    /// that created the iCalendar. The default is <see cref="LibraryMetadata.ProdId"/>.
    /// <para/>
    /// <b>Be careful when setting a custom value</b>, as it is free-form text that must conform to the iCalendar specification
    /// (RFC 5545 Section 3.7.3). The product ID must be specified.
    /// <para/>
    /// The default value does not apply to deserialized objects.
    /// </summary>
    public virtual string ProductId
    {
        get => Properties.Get<string>("PRODID");
        set => Properties.Set("PRODID", value);
    }

    public virtual string Scale
    {
        get => Properties.Get<string>("CALSCALE");
        set => Properties.Set("CALSCALE", value);
    }

    public virtual string Method
    {
        get => Properties.Get<string>("METHOD");
        set => Properties.Set("METHOD", value);
    }

    /// <summary>
    /// Adds a time zone to the iCalendar.  This time zone may
    /// then be used in date/time objects contained in the
    /// calendar.
    /// </summary>
    /// <returns>The time zone added to the calendar.</returns>
    public VTimeZone AddTimeZone(VTimeZone tz)
    {
        this.AddChild(tz);
        return tz;
    }

    /// <summary>
    /// Returns a list of occurrences of each recurring component
    /// that occur between <paramref name="startTime"/> and <paramref name="endTime"/>.
    /// </summary>
    /// <param name="startTime">The beginning date/time of the range.</param>
    /// <param name="endTime">The end date/time of the range.</param>
    /// <returns>A list of occurrences that fall between the date/time arguments provided.</returns>
    public virtual IEnumerable<Occurrence> GetOccurrences(CalDateTime startTime = null, CalDateTime endTime = null, EvaluationOptions options = default)
        => GetOccurrences<IRecurringComponent>(startTime, endTime, options);

    /// <summary>
    /// Returns all occurrences of components of type T that start within the date range provided.
    /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
    /// will be returned.
    /// </summary>
    /// <param name="startTime">The starting date range</param>
    /// <param name="endTime">The ending date range</param>
    public virtual IEnumerable<Occurrence> GetOccurrences<T>(CalDateTime startTime = null, CalDateTime endTime = null, EvaluationOptions options = default) where T : IRecurringComponent
    {
        // These are the UID/RECURRENCE-ID combinations that replace other occurrences.
        var recurrenceIdsAndUids = this.Children.OfType<IRecurrable>()
            .Where(r => r.RecurrenceId != null)
            .Select(r => new { (r as IUniqueComponent)?.Uid, Dt = r.RecurrenceId.Value })
            .Where(r => r.Uid != null)
            .ToDictionary(x => x);

        var occurrences = RecurringItems
            .OfType<T>()
            .Select(recurrable => recurrable.GetOccurrences(startTime, endTime, options))

            // Enumerate the list of occurrences (not the occurrences themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList() //NOSONAR - deliberately enumerate here

            // Merge the individual sequences into a single one. Take advantage of them
            // being ordered to avoid full enumeration.
            .OrderedMergeMany()

            // Remove duplicates and take advantage of being ordered to avoid full enumeration.
            .OrderedDistinct()

            // Remove the occurrence if it has been replaced by a different one.
            .Where(r =>
                (r.Source.RecurrenceId != null) ||
                !(r.Source is IUniqueComponent) ||
                !recurrenceIdsAndUids.ContainsKey(new { ((IUniqueComponent)r.Source).Uid, Dt = r.Period.StartTime.Value }));

        return occurrences;
    }

    /// <summary>
    /// Creates a typed object that is a direct child of the iCalendar itself.  Generally,
    /// you would invoke this method to create an Event, Todo, Journal, VTimeZone, FreeBusy,
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
        if (Activator.CreateInstance(typeof(T), true) is ICalendarObject cal)
        {
            this.AddChild(cal);
            return (T) cal;
        }
        return default(T);
    }

    public virtual void MergeWith(IMergeable obj)
    {
        var c = obj as Calendar;
        if (c == null)
        {
            return;
        }

        if (Name == null)
        {
            Name = c.Name;
        }

        Method = c.Method;
        Version = c.Version;
        ProductId = c.ProductId;
        Scale = c.Scale;

        foreach (var p in c.Properties.Where(p => !Properties.ContainsKey(p.Name)))
        {
            Properties.Add(p);
        }

        foreach (var child in c.Children)
        {
            if (child is IUniqueComponent)
            {
                if (!UniqueComponents.ContainsKey(((IUniqueComponent) child).Uid))
                {
                    this.AddChild(child);
                }
            }
            else
            {
                this.AddChild(child);
            }
        }
    }

    public virtual FreeBusy GetFreeBusy(FreeBusy freeBusyRequest) => CalendarComponents.FreeBusy.Create(this, freeBusyRequest);

    public virtual FreeBusy GetFreeBusy(CalDateTime fromInclusive, CalDateTime toExclusive)
        => CalendarComponents.FreeBusy.Create(this, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, null, null));

    public virtual FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, CalDateTime fromInclusive, CalDateTime toExclusive)
        => CalendarComponents.FreeBusy.Create(this, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, organizer, contacts));

    /// <summary>
    /// Adds a system time zone to the iCalendar.  This time zone may
    /// then be used in date/time objects contained in the
    /// calendar.
    /// </summary>
    /// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
    /// <returns>The time zone added to the calendar.</returns>
    public VTimeZone AddTimeZone(TimeZoneInfo tzi)
    {
        var tz = VTimeZone.FromSystemTimeZone(tzi);
        this.AddChild(tz);
        return tz;
    }

    public VTimeZone AddTimeZone(TimeZoneInfo tzi, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
    {
        var tz = VTimeZone.FromSystemTimeZone(tzi, earliestDateTimeToSupport, includeHistoricalData);
        this.AddChild(tz);
        return tz;
    }

    public VTimeZone AddTimeZone(string tzId)
    {
        var tz = VTimeZone.FromDateTimeZone(tzId);
        this.AddChild(tz);
        return tz;
    }

    public VTimeZone AddTimeZone(string tzId, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
    {
        var tz = VTimeZone.FromDateTimeZone(tzId, earliestDateTimeToSupport, includeHistoricalData);
        this.AddChild(tz);
        return tz;
    }

    public VTimeZone AddLocalTimeZone(DateTime earliestDateTimeToSupport, bool includeHistoricalData)
    {
        var tz = VTimeZone.FromLocalTimeZone(earliestDateTimeToSupport, includeHistoricalData);
        this.AddChild(tz);
        return tz;
    }
}
