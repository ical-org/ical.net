//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;

namespace Ical.Net;

/// <summary>
/// A list of iCalendars.
/// </summary>
public class CalendarCollection : List<Calendar>
{
    public static CalendarCollection Load(string iCalendarString)
        => Load(new StringReader(iCalendarString));

    /// <summary>
    /// Loads an <see cref="Calendar"/> from an open stream.
    /// </summary>
    /// <param name="s">The stream from which to load the <see cref="Calendar"/> object</param>
    /// <returns>An <see cref="Calendar"/> object</returns>
    public static CalendarCollection Load(Stream s)
        => Load(new StreamReader(s, Encoding.UTF8));

    public static CalendarCollection Load(TextReader tr)
    {
        var calendars = SimpleDeserializer.Default.Deserialize(tr).OfType<Calendar>();
        var collection = new CalendarCollection();
        collection.AddRange(calendars);
        return collection;
    }

    private IEnumerable<Occurrence> GetOccurrences(Func<Calendar, IEnumerable<Occurrence>> f)
        =>

        // Get the sequence of occurrences for each calendar in the collection,
        // which will result in a sequence of sequences of occurrences.
        this.Select(f)

        // Enumerate the list of occurrences (not the occurrences themselves) now to ensure
        // the initialization code is run, including validation and error handling.
        // This way we receive validation errors early, not only when enumeration starts.
        .ToArray()

        // Merge the individual sequences into a single one. Take advantage of them
        // being ordered to avoid full enumeration.
        .OrderedMergeMany();

    public IEnumerable<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        => GetOccurrences(iCal => iCal.GetOccurrences(startTime, endTime));

    public IEnumerable<Occurrence> GetOccurrences(DateTime? startTime, DateTime? endTime)
        => GetOccurrences(iCal => iCal.GetOccurrences(startTime, endTime));

    public IEnumerable<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        => GetOccurrences(iCal => iCal.GetOccurrences<T>(startTime, endTime));

    public IEnumerable<Occurrence> GetOccurrences<T>(DateTime? startTime, DateTime? endTime) where T : IRecurringComponent
        => GetOccurrences(iCal => iCal.GetOccurrences<T>(startTime, endTime));

    private FreeBusy CombineFreeBusy(FreeBusy main, FreeBusy current)
    {
        main?.MergeWith(current);
        return current;
    }

    public FreeBusy GetFreeBusy(FreeBusy freeBusyRequest)
    {
        return this.Aggregate<Calendar, FreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(freeBusyRequest)));
    }

    public FreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive)
    {
        return this.Aggregate<Calendar, FreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(fromInclusive, toExclusive)));
    }

    public FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, IDateTime fromInclusive, IDateTime toExclusive)
    {
        return this.Aggregate<Calendar, FreeBusy>(null, (current, iCal) => CombineFreeBusy(current, iCal.GetFreeBusy(organizer, contacts, fromInclusive, toExclusive)));
    }

    public override int GetHashCode() => CollectionHelpers.GetHashCode(this);

    protected bool Equals(CalendarCollection obj) => CollectionHelpers.Equals(this, obj);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((CalendarEvent) obj);
    }
}
