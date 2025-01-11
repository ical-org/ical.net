//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
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
public class CalendarEvent : RecurringComponent, IAlarmContainer, IComparable<CalendarEvent>
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
    public virtual IDateTime? DtEnd
    {
        get => Properties.Get<IDateTime?>("DTEND");
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
        get => Properties.Get<Duration?>("DURATION");
        set
        {
            if (DtEnd is not null) throw new InvalidOperationException("DURATION property cannot be set when DTEND property is not null.");
            SetProperty("DURATION", value);
        }
    }

    /// <summary>
    /// Gets the time span that gets added to the period start time to get the period end time.
    /// <para/>
    /// If the <see cref="CalendarEvent.Duration"/> property is not null, its value will be returned.<br/>
    /// If <see cref="RecurringComponent.DtStart"/> and <see cref="CalendarEvent.DtEnd"/> are set, it will return <see cref="CalendarEvent.DtEnd"/> minus <see cref="CalendarEvent.DtStart"/>.<br/>
    /// Otherwise, it will return <see cref="Duration.Zero"/>.
    /// </summary>
    /// <remarks>
    /// Note: For recurring events, the <b>exact duration</b> of individual occurrences may vary due to DST transitions
    /// of the given <see cref="RecurringComponent.DtStart"/> and <see cref="CalendarEvent.DtEnd"/> timezones.
    /// </remarks>
    /// <returns>The time span that gets added to the period start time to get the period end time.</returns>
    internal Duration GetEffectiveDuration()
    {
        // 3.8.5.3. Recurrence Rule
        // If the duration of the recurring component is specified with the
        // "DURATION" property, then the same NOMINAL duration will apply to
        // all the members of the generated recurrence set and the exact
        // duration of each recurrence instance will depend on its specific
        // start time.
        if (Duration is not null)
            return Duration.Value;

        if (DtStart is not { } dtStart)
        {
            // Mustn't happen
            throw new InvalidOperationException("DtStart must be set.");
        }

        if (DtEnd is not null)
        {
            /*
                The 'DTEND' property for a 'VEVENT' calendar component specifies the
                non-inclusive end of the event.

                3.8.5.3. Recurrence Rule:
                If the duration of the recurring component is specified with the
                "DTEND" or "DUE" property, then the same EXACT duration will apply
                to all the members of the generated recurrence set.

                We use the difference from DtStart to DtEnd (neglecting timezone),
                because the caller will set the period end time to the
                same timezone as the event end time. This finally leads to an exact duration
                calculation from the zoned start time to the zoned end time.
             */
            return DtEnd.Subtract(dtStart);
        }

        // RFC 5545 3.6.1:
        // For cases where a "VEVENT" calendar component
        // specifies a "DTSTART" property with a DATE value type but no
        // "DTEND" nor "DURATION" property, the event’s duration is taken to
        // be one day.
        // This is taken care of in the PeriodSerializer class,
        // so we don't use magic numbers here.

        // For DtStart.HasTime but no DtEnd - also the default case
        //
        // RFC 5545 3.6.1:
        // For cases where a "VEVENT" calendar component
        // specifies a "DTSTART" property with a DATE-TIME value type but no
        // "DTEND" property, the event ends on the same calendar date and
        // time of day specified by the "DTSTART" property.
        return DataTypes.Duration.Zero;
    }

    /// <summary>
    /// An alias to the DtEnd field (i.e. end date/time).
    /// </summary>
    public virtual IDateTime? End
    {
        get => DtEnd;
        set => DtEnd = value;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the event is an all-day event,
    /// meaning the <see cref="RecurringComponent.Start"/> is a 'DATE' value type.
    /// </summary>
    public virtual bool IsAllDay => !Start.HasTime;

    /// <summary>
    /// The geographic location (lat/long) of the event.
    /// </summary>
    public virtual GeographicLocation? GeographicLocation
    {
        get => Properties.Get<GeographicLocation?>("GEO");
        set => SetProperty("GEO", value);
    }

    /// <summary>
    /// The location of the event.
    /// </summary>
    public virtual string? Location
    {
        get => Properties.Get<string?>("LOCATION");
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
        get => Properties.Get<string?>("STATUS");
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
        get => Properties.Get<string?>(TransparencyType.Key);
        set => SetProperty(TransparencyType.Key, value);
    }

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
        SetService(new EventEvaluator(this));
    }

    /// <summary>
    /// Determines whether the <see cref="CalendarEvent"/> is actively displayed
    /// as an upcoming or occurred event.
    /// </summary>
    /// <returns>True if the event has not been cancelled, False otherwise.</returns>
    public virtual bool IsActive => !string.Equals(Status, EventStatus.Cancelled, EventStatus.Comparison);

    /// <inheritdoc/>
    protected override bool EvaluationIncludesReferenceDate => true;

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
            && EvaluationIncludesReferenceDate == other.EvaluationIncludesReferenceDate
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

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((CalendarEvent)obj);
    }

    /// <inheritdoc/>
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
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime));
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionRules);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceDates.GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime));
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceRules);
            return hashCode;
        }
    }

    /// <inheritdoc/>
    public int CompareTo(CalendarEvent? other)
    {
        if (other is null)
        {
            return 1;
        }
        if (DtStart.Equals(other.DtStart))
        {
            return 0;
        }
        if (DtStart.LessThan(other.DtStart))
        {
            return -1;
        }

        // meaning DtStart is greater than other
        return 1;
    }
}
