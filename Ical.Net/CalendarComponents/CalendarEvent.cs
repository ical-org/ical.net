//
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
/// <remarks>
/// The following properties are not supported by this class:
/// <para/>
/// - Organizer and Attendee properties
/// - Class property
/// - Priority property
/// - Related property
/// - TextCollection DataType for 'text' items separated by commas
/// </remarks>
public class CalendarEvent : RecurringComponent, IAlarmContainer
{
    internal const string ComponentName = "VEVENT";

    private void SetProperty<T>(string group, T value)
    {
        if (value is not null)
        {
            Properties.Set(group, value);
            return;
        }

        Properties.Remove(group);
    }

    /// <summary>
    /// The end date/time of the event.
    /// <para/>
    /// Either <see cref="Duration"/> OR <see cref="DtEnd"/> can be present in the event, but not both.
    /// <note>
    /// If the duration has not been set, but
    /// the start/end time of the event is available,
    /// the duration is automatically determined.
    /// Likewise, if an end time and duration are available,
    /// but a start time has not been set, the start time
    /// will be extrapolated.
    /// </note>
    /// </summary>
    public virtual CalDateTime? DtEnd
    {
        get => Properties.Get<CalDateTime>("DTEND");
        set
        {
            if (Duration is not null) throw new InvalidOperationException("DTEND property cannot be set when DURATION property is not null.");
            SetProperty("DTEND", value);
        }
    }

    /// <summary>
    /// The duration of the event.
    /// <para/>
    /// Either <see cref="Duration"/> OR <see cref="DtEnd"/> can be present in the event, but not both.
    /// </summary>
    /// <remarks>
    /// If a start time and duration is available,
    /// the end time is automatically determined.
    /// Likewise, the duration is determined with start/end times
    /// if it is not explicitly set.
    /// <para/>
    /// RFC 5545 states:
    /// Either 'dtend' or 'duration' may appear in
    /// an 'eventprop', but 'dtend' and 'duration'
    /// MUST NOT occur in the same 'eventprop'
    /// </remarks>
    public virtual Duration? Duration
    {
        // Duration is a struct, so we need to use Nullable<Duration> to allow null values
        get => Properties.Get<Duration?>("DURATION");
        set
        {
            if (DtEnd is not null) throw new InvalidOperationException("DURATION property cannot be set when DTEND property is not null.");
            SetProperty("DURATION", value);
        }
    }

    /// <summary>
    /// An alias to the DtEnd field (i.e. end date/time).
    /// </summary>
    public virtual CalDateTime? End
    {
        get => DtEnd;
        set => DtEnd = value;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the event is an all-day event,
    /// meaning the <see cref="RecurringComponent.Start"/> is a 'DATE' value type.
    /// </summary>
    public virtual bool IsAllDay => Start?.HasTime == false;

    /// <summary>
    /// The geographic location (lat/long) of the event.
    /// </summary>
    public virtual GeographicLocation? GeographicLocation
    {
        get => Properties.Get<GeographicLocation>("GEO");
        set => SetProperty("GEO", value);
    }

    /// <summary>
    /// The location of the event.
    /// </summary>
    public virtual string? Location
    {
        get => Properties.Get<string>("LOCATION");
        set => SetProperty("LOCATION", value);
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
        set => Properties.Set("RESOURCES", value);
    }

    /// <summary>
    /// The STATUS property of the event.
    /// </summary>
    public virtual string? Status
    {
        get => Properties.Get<string>("STATUS");
        set => SetProperty("STATUS", value);
    }

    /// <summary>
    /// The transparency of the event. Determines,
    /// whether the period of time this event
    /// occupies may overlap with other events (transparent),
    /// or if the time cannot be scheduled for anything
    /// else (opaque).
    /// </summary>
    public virtual string? Transparency
    {
        get => Properties.Get<string>(TransparencyType.Key);
        set => SetProperty(TransparencyType.Key, value);
    }

    /// <summary>
    /// Constructs an Event object, with an iCalObject
    /// (usually an iCalendar object) as its parent.
    /// </summary>
    public CalendarEvent()
    {
        this.Evaluator = new EventEvaluator(this);
        Initialize();
    }

    private void Initialize()
    {
        Name = EventStatus.Name;
    }

    /// <summary>
    /// Determines whether the <see cref="CalendarEvent"/> is actively displayed
    /// as an upcoming or occurred event.
    /// </summary>
    /// <returns>True if the event has not been cancelled, False otherwise.</returns>
    public virtual bool IsActive => !string.Equals(Status, EventStatus.Cancelled, EventStatus.Comparison);

    public override IEvaluator Evaluator { get; }

    /// <inheritdoc/>
    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);

        Initialize();
    }

    protected bool Equals(CalendarEvent? other)
    {
        if (other is null) return false;

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
            && Attachments.SequenceEqual(other.Attachments)
            && CollectionHelpers.Equals(ExceptionRules, other.ExceptionRules)
            && CollectionHelpers.Equals(RecurrenceRules, other.RecurrenceRules);

        if (!result)
        {
            return false;
        }

        // exDates and otherExDates are filled with a sorted list of distinct periods
        var exDates = ExceptionDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime).ToList();
        var otherExDates = other.ExceptionDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime).ToList();
        if (exDates.Count != otherExDates.Count || !exDates.SequenceEqual(otherExDates))
        {
            return false;
        }

        // rDates and otherRDates are filled with a sorted list of distinct periods
        var rDates = RecurrenceDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime).ToList();
        var otherRDates = other.RecurrenceDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime).ToList();
        if (rDates.Count != otherRDates.Count && !rDates.SequenceEqual(otherRDates))
        {
            return false;
        }

        return true;
    }
}
