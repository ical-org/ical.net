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
using Ical.Net.Proxies;
using NodaTime;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// An iCalendar component that recurs.
/// </summary>
/// <remarks>
/// This component automatically handles
/// RRULEs, RDATE, and EXDATEs, as well as the DTSTART
/// for the recurring item (all recurring items must have a DTSTART).
/// </remarks>
public abstract class RecurringComponent : UniqueComponent, IRecurringComponent
{
    public static IEnumerable<IRecurringComponent> SortByDate(IEnumerable<IRecurringComponent> list) => SortByDate<IRecurringComponent>(list);

    public static IEnumerable<TRecurringComponent> SortByDate<TRecurringComponent>(IEnumerable<TRecurringComponent> list) => list.OrderBy(d => d);

    public virtual IList<Attachment> Attachments
    {
        get => Properties.GetMany<Attachment>("ATTACH");
        set => Properties.Set("ATTACH", value);
    }

    public virtual IList<string> Categories
    {
        get => Properties.GetMany<string>("CATEGORIES");
        set => Properties.Set("CATEGORIES", value);
    }

    public virtual string? Class
    {
        get => Properties.Get<string>("CLASS");
        set => Properties.Set("CLASS", value);
    }

    public virtual IList<string> Contacts
    {
        get => Properties.GetMany<string>("CONTACT");
        set => Properties.Set("CONTACT", value);
    }

    public virtual CalDateTime? Created
    {
        get => Properties.Get<CalDateTime>("CREATED");
        set => Properties.Set("CREATED", value);
    }

    public virtual string? Description
    {
        get => Properties.Get<string>("DESCRIPTION");
        set => Properties.Set("DESCRIPTION", value);
    }

    /// <summary>
    /// The start date/time of the component.
    /// </summary>
    public virtual CalDateTime? DtStart
    {
        get => Properties.Get<CalDateTime>("DTSTART");
        set => Properties.Set("DTSTART", value);
    }

    internal IList<PeriodList> ExceptionDatesPeriodLists
    {
        get => Properties.GetMany<PeriodList>("EXDATE");
        set => Properties.Set("EXDATE", value);
    }

    public virtual ExceptionDates ExceptionDates { get; internal set; } = null!;

    public virtual CalDateTime? LastModified
    {
        get => Properties.Get<CalDateTime>("LAST-MODIFIED");
        set => Properties.Set("LAST-MODIFIED", value);
    }

    public virtual int Priority
    {
        get => Properties.Get<int>("PRIORITY");
        set => Properties.Set("PRIORITY", value);
    }

    internal virtual IList<PeriodList> RecurrenceDatesPeriodLists
    {
        get => Properties.GetMany<PeriodList>("RDATE");
        set => Properties.Set("RDATE", value);
    }

    public virtual RecurrenceDates RecurrenceDates { get; internal set; } = null!;

    public virtual RecurrenceRule? RecurrenceRule
    {
        get => Properties.Get<RecurrenceRule>("RRULE");
        set => Properties.Set("RRULE", value);
    }

    /// <summary>
    /// Gets or sets the recurrence identifier for a specific instance of a recurring event.
    /// </summary>
    /// <remarks>Use <see cref="RecurrenceIdentifier"/> instead, which
    /// supports the RANGE parameter for recurring events.</remarks>
    [Obsolete("Use RecurrenceIdentifier instead, which supports the RANGE parameter.")]
    public virtual CalDateTime? RecurrenceId
    {
        get => RecurrenceIdentifier?.Range == RecurrenceRange.ThisInstance ? RecurrenceIdentifier.StartTime : null;
        set => RecurrenceIdentifier = value is null ? null : new RecurrenceIdentifier(value, RecurrenceRange.ThisInstance);
    }

    /// <summary>
    /// Gets or sets the recurrence identifier for a specific instance of a recurring event.
    /// <para/>
    /// The <see cref="RecurrenceIdentifier.Range"/> sets the scope of the recurrence instance:
    /// With <see cref="RecurrenceRange.ThisInstance"/>, the instance is limited to the specific
    /// occurrence identified by the <see cref="RecurrenceIdentifier.StartTime"/>.<br/>
    /// With <see cref="RecurrenceRange.ThisAndFuture"/>, the instance applies to the specified
    /// <see cref="RecurrenceIdentifier.StartTime"/> and all future occurrences.
    /// </summary>
    public virtual RecurrenceIdentifier? RecurrenceIdentifier
    {
        get => Properties.Get<RecurrenceIdentifier>("RECURRENCE-ID");
        set => Properties.Set("RECURRENCE-ID", value);
    }

    public virtual IList<string> RelatedComponents
    {
        get => Properties.GetMany<string>("RELATED-TO");
        set => Properties.Set("RELATED-TO", value);
    }

    public virtual int Sequence
    {
        get => Properties.Get<int>("SEQUENCE");
        set => Properties.Set("SEQUENCE", value);
    }

    /// <summary>
    /// An alias to the DTStart field (i.e. start date/time).
    /// </summary>
    public virtual CalDateTime? Start
    {
        get => DtStart;
        set => DtStart = value;
    }

    public virtual string? Summary
    {
        get => Properties.Get<string>("SUMMARY");
        set => Properties.Set("SUMMARY", value);
    }

    /// <summary>
    /// A list of <see cref="Alarm"/>s for this recurring component.
    /// </summary>
    public virtual ICalendarObjectList<Alarm> Alarms => new CalendarObjectListProxy<Alarm>(Children);

    public abstract IEvaluator? Evaluator { get; }

    protected RecurringComponent()
    {
        Initialize();
        EnsureProperties();
    }

    protected RecurringComponent(string name) : base(name)
    {
        Initialize();
        EnsureProperties();
    }

    private void Initialize()
    {
        ExceptionDates = new ExceptionDates(ExceptionDatesPeriodLists);
        RecurrenceDates = new RecurrenceDates(RecurrenceDatesPeriodLists);
    }

    private void EnsureProperties()
    {
        if (!Properties.ContainsKey("SEQUENCE"))
        {
            Sequence = 0;
        }
    }

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);

        Initialize();
    }

    public virtual IEnumerable<Occurrence> GetOccurrences(ZonedDateTime startTime, EvaluationOptions? options = null)
        => RecurrenceUtil.GetOccurrences(this, startTime.Zone, startTime.ToInstant(), options);

    public virtual IEnumerable<Occurrence> GetOccurrences(DateTimeZone timeZone, Instant? startTime = null, EvaluationOptions? options = null)
    {
        return RecurrenceUtil.GetOccurrences(this, timeZone, startTime, options);
    }

    public virtual IList<AlarmOccurrence> PollAlarms(DateTimeZone timeZone) => PollAlarms(timeZone, null, null);

    public virtual IList<AlarmOccurrence> PollAlarms(DateTimeZone timeZone, Instant? startTime, Instant? endTime)
        => Alarms.SelectMany(a => a.Poll(timeZone, startTime).TakeWhile(p => (endTime == null) || (p.Start.ToInstant() < endTime))).ToList();
}
