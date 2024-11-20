﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Utility;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// A class that represents an RFC 5545 VEVENT component.
/// </summary>
/// <note>
///     TODO: Add support for the following properties:
///     <list type="bullet">
///         <item>Add support for the Organizer and Attendee properties</item>
///         <item>Add support for the Class property</item>
///         <item>Add support for the Geo property</item>
///         <item>Add support for the Priority property</item>
///         <item>Add support for the Related property</item>
///         <item>Create a TextCollection DataType for 'text' items separated by commas</item>
///     </list>
/// </note>
public class CalendarEvent : RecurringComponent, IAlarmContainer, IComparable<CalendarEvent>
{
    internal const string ComponentName = "VEVENT";

    /// <summary>
    /// The end date/time of the event.
    /// <note>
    /// If the duration has not been set, but
    /// the start/end time of the event is available,
    /// the duration is automatically determined.
    /// Likewise, if an end time and duration are available,
    /// but a start time has not been set, the start time
    /// will be extrapolated.
    /// </note>
    /// </summary>
    public virtual IDateTime DtEnd
    {
        get => Properties.Get<IDateTime>("DTEND");
        set
        {
            if (!Equals(DtEnd, value))
            {
                Properties.Set("DTEND", value);
            }
        }
    }

    /// <summary>
    /// The duration of the event.
    /// <note>
    /// If a start time and duration is available,
    /// the end time is automatically determined.
    /// Likewise, if the end time and duration is
    /// available, but a start time is not determined,
    /// the start time will be extrapolated from
    /// available information.
    /// </note>
    /// </summary>
    // NOTE: Duration is not supported by all systems,
    // (i.e. iPhone) and cannot co-exist with DtEnd.
    // RFC 5545 states:
    //
    //      ; either 'dtend' or 'duration' may appear in
    //      ; a 'eventprop', but 'dtend' and 'duration'
    //      ; MUST NOT occur in the same 'eventprop'
    //
    // Therefore, Duration is not serialized, as DtEnd
    // should always be extrapolated from the duration.
    public virtual TimeSpan Duration
    {
        get => Properties.Get<TimeSpan>("DURATION");
        set
        {
            if (!Equals(Duration, value))
            {
                Properties.Set("DURATION", value);
            }
        }
    }

    /// <summary>
    /// Calculates and returns the duration of the first occurrence of this event.
    /// </summary>
    /// <remarks>
    /// If the 'DURATION' property is set, this method will return its value.
    /// Otherwise, if DTSTART and DTEND are set, it will return DTSTART minus DTEND.
    /// Otherwise it will return `default(TimeSpan)`.
    /// Note that for recurring events, the duration of individual occurrences may vary
    /// if they span a DST change.
    /// </remarks>
    /// <returns>The effective duration of this event.</returns>
    public virtual TimeSpan GetFirstDuration()
    {
        if (Properties.ContainsKey("DURATION"))
            return Duration;

        if (DtStart is not null)
        {
            if (DtEnd is not null)
            {
                // The "DTEND" property
                // for a "VEVENT" calendar component specifies the non-inclusive end
                // of the event.
                return DtEnd.Subtract(DtStart);
            }
            else if (!DtStart.HasTime)
            {
                // For cases where a "VEVENT" calendar component
                // specifies a "DTSTART" property with a DATE value type but no
                // "DTEND" nor "DURATION" property, the event's duration is taken to
                // be one day.
                return TimeSpan.FromDays(1);
            }
            else
            {
                // For cases where a "VEVENT" calendar component
                // specifies a "DTSTART" property with a DATE-TIME value type but no
                // "DTEND" property, the event ends on the same calendar date and
                // time of day specified by the "DTSTART" property.
                return TimeSpan.Zero;
            }
        }

        // This is an illegal state. We return zero for compatibility reasons.
        return TimeSpan.Zero;
    }


    /// <summary>
    /// An alias to the DtEnd field (i.e. end date/time).
    /// </summary>
    public virtual IDateTime End
    {
        get => DtEnd;
        set => DtEnd = value;
    }

    /// <summary>
    /// Returns true if the event is an all-day event.
    /// </summary>
    public virtual bool IsAllDay
    {
        get => !Start.HasTime;
        set
        {
            // Set whether or not the start date/time
            // has a time value.
            if (Start != null)
            {
                Start.HasTime = !value;
            }
            if (End != null)
            {
                End.HasTime = !value;
            }

            if (value && Start != null && End != null && Equals(Start.Date, End.Date))
            {
                Duration = default(TimeSpan);
                End = Start.AddDays(1);
            }
        }
    }

    /// <summary>
    /// The geographic location (lat/long) of the event.
    /// </summary>
    public GeographicLocation GeographicLocation
    {
        get => Properties.Get<GeographicLocation>("GEO");
        set => Properties.Set("GEO", value);
    }

    /// <summary>
    /// The location of the event.
    /// </summary>
    public string Location
    {
        get => Properties.Get<string>("LOCATION");
        set => Properties.Set("LOCATION", value);
    }

    /// <summary>
    /// Resources that will be used during the event.
    /// To change existing values, assign a new <see cref="IList{T}"/>.
    /// <example>Examples:
    /// Conference room, Projector
    /// </example>
    /// </summary>
    public virtual IList<string> Resources
    {
        get => Properties.GetMany<string>("RESOURCES");
        set => Properties.Set("RESOURCES", value ?? new List<string>());
    }

    /// <summary>
    /// The status of the event.
    /// </summary>
    public string Status
    {
        get => Properties.Get<string>("STATUS");
        set => Properties.Set("STATUS", value);
    }

    /// <summary>
    /// The transparency of the event.  In other words,
    /// whether or not the period of time this event
    /// occupies can contain other events (transparent),
    /// or if the time cannot be scheduled for anything
    /// else (opaque).
    /// </summary>
    public string Transparency
    {
        get => Properties.Get<string>(TransparencyType.Key);
        set => Properties.Set(TransparencyType.Key, value);
    }

    private EventEvaluator _mEvaluator;

    /// <summary>
    /// Constructs an Event object, with an iCalObject
    /// (usually an iCalendar object) as its parent.
    /// </summary>
    public CalendarEvent()
    {
        Initialize();
    }

    private void Initialize()
    {
        Name = EventStatus.Name;

        _mEvaluator = new EventEvaluator(this);
        SetService(_mEvaluator);
    }

    /// <summary>
    /// Use this method to determine if an event occurs on a given date.
    /// <note type="caution">
    ///     This event should be called only after the Evaluate
    ///     method has calculated the dates for which this event occurs.
    /// </note>
    /// </summary>
    /// <param name="dateTime">The date to test.</param>
    /// <returns>True if the event occurs on the <paramref name="dateTime"/> provided, False otherwise.</returns>
    public virtual bool OccursOn(IDateTime dateTime)
    {
        return _mEvaluator.Periods.Any(p => p.StartTime.Date == dateTime.Date || // It's the start date OR
                                            (p.StartTime.Date <= dateTime.Date && // It's after the start date AND
                                             (p.EndTime.HasTime && p.EndTime.Date >= dateTime.Date || // an end time was specified, and it's after the test date
                                              (!p.EndTime.HasTime && p.EndTime.Date > dateTime.Date))));
    }

    /// <summary>
    /// Use this method to determine if an event begins at a given date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to test.</param>
    /// <returns>True if the event begins at the given date and time</returns>
    public virtual bool OccursAt(IDateTime dateTime)
    {
        return _mEvaluator.Periods.Any(p => p.StartTime.Equals(dateTime));
    }

    /// <summary>
    /// Determines whether or not the <see cref="CalendarEvent"/> is actively displayed
    /// as an upcoming or occurred event.
    /// </summary>
    /// <returns>True if the event has not been cancelled, False otherwise.</returns>
    public virtual bool IsActive => !string.Equals(Status, EventStatus.Cancelled, EventStatus.Comparison);

    protected override bool EvaluationIncludesReferenceDate => true;

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);

        Initialize();
    }

    protected override void OnDeserialized(StreamingContext context)
    {
        base.OnDeserialized(context);
    }

    protected bool Equals(CalendarEvent other)
    {
        var resourcesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        resourcesSet.UnionWith(Resources);

        var result =
            Equals(DtStart, other.DtStart)
            && string.Equals(Summary, other.Summary, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase)
            && Equals(DtEnd, other.DtEnd)
            && Equals(Duration, other.Duration)
            && string.Equals(Location, other.Location, StringComparison.OrdinalIgnoreCase)
            && resourcesSet.SetEquals(other.Resources)
            && string.Equals(Status, other.Status, StringComparison.Ordinal)
            && IsActive == other.IsActive
            && string.Equals(Transparency, other.Transparency, TransparencyType.Comparison)
            && EvaluationIncludesReferenceDate == other.EvaluationIncludesReferenceDate
            && Attachments.SequenceEqual(other.Attachments)
            && CollectionHelpers.Equals(ExceptionRules, other.ExceptionRules)
            && CollectionHelpers.Equals(RecurrenceRules, other.RecurrenceRules);

        if (!result)
        {
            return false;
        }

        //RDATEs and EXDATEs are all List<PeriodList>, because the spec allows for multiple declarations of collections.
        //Consequently we have to contrive a normalized representation before we can determine whether two events are equal

        var exDates = PeriodList.GetGroupedPeriods(ExceptionDates);
        var otherExDates = PeriodList.GetGroupedPeriods(other.ExceptionDates);
        if (exDates.Keys.Count != otherExDates.Keys.Count || !exDates.Keys.OrderBy(k => k).SequenceEqual(otherExDates.Keys.OrderBy(k => k)))
        {
            return false;
        }

        if (exDates.Any(exDate => !exDate.Value.OrderBy(d => d).SequenceEqual(otherExDates[exDate.Key].OrderBy(d => d))))
        {
            return false;
        }

        var rDates = PeriodList.GetGroupedPeriods(RecurrenceDates);
        var otherRDates = PeriodList.GetGroupedPeriods(other.RecurrenceDates);
        if (rDates.Keys.Count != otherRDates.Keys.Count || !rDates.Keys.OrderBy(k => k).SequenceEqual(otherRDates.Keys.OrderBy(k => k)))
        {
            return false;
        }

        if (rDates.Any(exDate => !exDate.Value.OrderBy(d => d).SequenceEqual(otherRDates[exDate.Key].OrderBy(d => d))))
        {
            return false;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((CalendarEvent)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = DtStart?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (Summary?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (DtEnd?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ Duration.GetHashCode();
            hashCode = (hashCode * 397) ^ (Location?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ Status?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ IsActive.GetHashCode();
            hashCode = (hashCode * 397) ^ Transparency?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Attachments);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Resources);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCodeForNestedCollection(ExceptionDates);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionRules);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCodeForNestedCollection(RecurrenceDates);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceRules);
            return hashCode;
        }
    }

    public int CompareTo(CalendarEvent other)
    {
        if (DtStart.Equals(other.DtStart))
        {
            return 0;
        }
        if (DtStart.LessThan(other.DtStart))
        {
            return -1;
        }
        if (DtStart.GreaterThan(other.DtStart))
        {
            return 1;
        }
        throw new Exception("An error occurred while comparing two CalDateTimes.");
    }
}
