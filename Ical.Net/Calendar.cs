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
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

public class Calendar : CalendarComponent, IGetOccurrencesTyped, IGetFreeBusy, IMergeable
{
    public static Calendar? Load(string iCalendarString)
        => CalendarCollection.Load(new StringReader(iCalendarString)).SingleOrDefault();

    /// <summary>
    /// Loads an <see cref="Calendar"/> from an open stream.
    /// </summary>
    /// <param name="s">The stream from which to load the <see cref="Calendar"/> object</param>
    /// <returns>An <see cref="Calendar"/> object</returns>
    public static Calendar? Load(Stream s)
        => CalendarCollection.Load(new StreamReader(s, Encoding.UTF8)).SingleOrDefault();

    public static Calendar? Load(TextReader tr)
        => CalendarCollection.Load(tr).SingleOrDefault();

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

        // Initialize the members to make the compiler happy.
        _mUniqueComponents = null!;
        _mEvents = null!;
        _mTodos = null!;
        _mJournals = null!;
        _mFreeBusy = null!;
        _mTimeZones = null!;

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
    public virtual string? Version
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
    public virtual string? ProductId
    {
        get => Properties.Get<string>("PRODID");
        set => Properties.Set("PRODID", value);
    }

    public virtual string? Scale
    {
        get => Properties.Get<string>("CALSCALE");
        set => Properties.Set("CALSCALE", value);
    }

    public virtual string? Method
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

    public virtual IEnumerable<Occurrence> GetOccurrences(ZonedDateTime startTime, EvaluationOptions? options = null)
    {
        return GetOccurrences<IRecurringComponent>(startTime, options);
    }

    public virtual IEnumerable<Occurrence> GetOccurrences(DateTimeZone timeZone, Instant? startTime = null, EvaluationOptions? options = null)
    {
        return GetOccurrences<IRecurringComponent>(timeZone, startTime, options);
    }

    public virtual IEnumerable<Occurrence> GetOccurrences<T>(ZonedDateTime startTime, EvaluationOptions? options = null) where T : IRecurringComponent
    {
        return GetOccurrences<T>(startTime.Zone, startTime.ToInstant(), options);
    }

    public virtual IEnumerable<Occurrence> GetOccurrences<T>(DateTimeZone timeZone, Instant? startTime = null, EvaluationOptions? options = null) where T : IRecurringComponent
    {
        // Get UID/RECURRENCE-ID combinations that replace occurrences
        var recurrenceIdsAndUids = GetRecurrenceIdsAndUids(timeZone, Children);

        var occurrences = RecurringItems
            .OfType<T>()
            .Select(recurrable => recurrable.GetOccurrences(timeZone, startTime, options)
                // Exclude occurrences that are overridden by other components with the same UID and RECURRENCE-ID.
                // This must happen before .OrderedDistinct() because that method would remove duplicates
                // based on the occurrence time, and we need to remove them based on UID + RECURRENCE-ID.
                .Where(r => IsUnmodifiedOccurrence(timeZone, r, recurrenceIdsAndUids)))

            // Enumerate the list of occurrences (not the occurrences themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList() //NOSONAR - deliberately enumerate here

            // Merge the individual sequences into a single one. Take advantage of them
            // being ordered to avoid full enumeration.
            .OrderedMergeMany()

            // Remove duplicates based on Period.StartTime and take advantage of
            // being ordered to avoid full enumeration.
            .OrderedDistinct()

            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return occurrences;
    }

    /// <summary>
    /// Gets the UID/RECURRENCE-ID combinations that replace other occurrences:
    /// Build a dictionary of overridden recurring components by their UID and RecurrenceId.
    /// <para/>
    /// This is used to identify the *latest* modification for each recurring instance.
    /// </summary>
    private Dictionary<(string Uid, Instant RecurrenceId), IUniqueComponent> GetRecurrenceIdsAndUids(
        DateTimeZone timeZone,
        IEnumerable<ICalendarObject> children)
    {
        var componentLookup = new Dictionary<(string Uid, Instant RecurrenceId), IUniqueComponent>();

        foreach (var r in children.OfType<IRecurrable>())
        {
            // Only ThisInstance is supported for now
            if (r.RecurrenceIdentifier is not { Range: RecurrenceRange.ThisInstance } rid)
            {
                continue;
            }

            // Ignore components without a UID
            if (r is not IUniqueComponent { Uid: not null } uc)
            {
                continue;
            }

            var key = GetOccurrenceKey(timeZone, uc.Uid, rid);

            // Use the maximum SEQUENCE if present, otherwise use the latest
            if (uc is CalendarEvent calEvent
                && componentLookup.TryGetValue(key, out var otherComponent)
                && otherComponent is CalendarEvent otherEvent
                && otherEvent.Sequence > calEvent.Sequence)
            {
                continue;
            }

            // Add or overwrite to use the latest component
            componentLookup[key] = uc;
        }

        return componentLookup;
    }

    /// <summary>
    /// Checks if an occurrence has not been replaced/overridden by a more
    /// recent modification (based on UID and RecurrenceId).
    /// </summary>
    private bool IsUnmodifiedOccurrence(
        DateTimeZone timeZone,
        Occurrence occurrence,
        Dictionary<(string Uid, Instant RecurrenceId), IUniqueComponent> recurrenceIdsAndUids)
    {
        if (occurrence.Source is not IUniqueComponent uc || uc.Uid is null)
        {
            // If not a unique component, always keep
            return true;
        }

        // Filter by range of ThisInstance because ThisAndFuture is not supported yet
        if (occurrence.Source.RecurrenceIdentifier is { } rid
            && rid.Range == RecurrenceRange.ThisInstance)
        {
            var key = GetOccurrenceKey(timeZone, uc.Uid, rid);

            // If the occurrence is a modified instance (has RecurrenceId and Uid)
            // and the source is the last modified instance for this RecurrenceId/Uid.
            return recurrenceIdsAndUids.TryGetValue(key, out var lastComponent)
                && ReferenceEquals(lastComponent, occurrence.Source);
        }

        // If not a modified occurrence, keep if:
        // - It is not a unique component, or
        // - There is no replacement for this UID/StartTime in recurrenceIdsAndUids
        return !recurrenceIdsAndUids.ContainsKey((uc.Uid, occurrence.Start.ToInstant()));
    }

    private (string, Instant) GetOccurrenceKey(DateTimeZone timeZone, string uid, RecurrenceIdentifier rid)
    {
        // Evaluate the RECURRENCE-ID start time as an Instant to identify the
        // exact occurrence, using the evaluation time zone for floating values.
        //
        // The RECURRENCE-ID value is supposed to be of the same type as the
        // DTSTART property of the master event. If the two properties are of
        // the DATE-TIME type, they could still have different time zones or
        // one could have a time zone while the other does not.
        //
        // If the master event has a time zone while the RECURRENCE-ID is
        // floating, evaluating the RECURRENCE-ID using the evaluation time zone
        // means that the RECURRENCE-ID could identify different occurrences
        // depending on the evaluation time zone.
        var startTime = rid.StartTime
            .ToZonedOrDefault(timeZone, TimeZoneProvider)
            .ToInstant();

        return (uid, startTime);
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
        throw new ArgumentException($"Creating {typeof(T).FullName} failed.");
    }

    public virtual void MergeWith(IMergeable obj)
    {
        if (obj is not Calendar c)
        {
            return;
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
            if (child is IUniqueComponent component)
            {
                if (component.Uid != null && !UniqueComponents.ContainsKey(component.Uid))
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

    public virtual FreeBusy? GetFreeBusy(DateTimeZone timeZone, FreeBusy freeBusyRequest) => CalendarComponents.FreeBusy.Create(this, timeZone, freeBusyRequest);

    public virtual FreeBusy? GetFreeBusy(DateTimeZone timeZone, CalDateTime fromInclusive, CalDateTime toExclusive)
        => CalendarComponents.FreeBusy.Create(this, timeZone, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, null, null));

    public virtual FreeBusy? GetFreeBusy(DateTimeZone timeZone, Organizer organizer, IEnumerable<Attendee> contacts, CalDateTime fromInclusive, CalDateTime toExclusive)
        => CalendarComponents.FreeBusy.Create(this, timeZone, CalendarComponents.FreeBusy.CreateRequest(fromInclusive, toExclusive, organizer, contacts));

    /// <summary>
    /// Adds a system time zone to the iCalendar. This time zone may
    /// then be used in date/time objects contained in the
    /// calendar.
    /// </summary>
    /// <param name="tzi">A <see cref="TimeZoneInfo"/> object to add to the calendar.</param>
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

#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _timeZoneProviderLock = new();
#else
    private readonly object _timeZoneProviderLock = new();
#endif

    public IDateTimeZoneProvider TimeZoneProvider
    {
        get
        {
            lock (_timeZoneProviderLock)
            {
                return field ??= CalendarTimeZoneProviders
                    .FromCalendar(this, CalendarTimeZoneProviders.TzdbWithAliases);
            }
        }
        set
        {
            lock (_timeZoneProviderLock) { field = value; }
        }
    }

    internal IDateTimeZoneSource TimeZoneSource => field ??= new CalendarDateTimeZoneSource(this);

    private sealed class CalendarDateTimeZoneSource(
        Calendar calendar) : IDateTimeZoneSource
    {
        public string VersionId => Components.Calendar;

        public DateTimeZone ForId(string id)
            => calendar._mTimeZones.FirstOrDefault(x => x.TzId == id)?.ToDateTimeZone()
            ?? throw new ArgumentException("Time zone ID not found");

        public IEnumerable<string> GetIds() => calendar._mTimeZones.Select(x => x.TzId!);

        public string? GetSystemDefaultId() => null;
    }
}
