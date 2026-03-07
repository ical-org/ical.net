//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;

namespace Ical.Net;

public class VTimeZoneInfo : CalendarComponent, IRecurrable
{
    private TimeZoneInfoEvaluator? _evaluator;

    public VTimeZoneInfo()
    {
        Initialize();
    }
    public VTimeZoneInfo(string name) : this()
    {
        Name = name;
        Initialize();
    }

    private void Initialize()
    {
        _evaluator = new TimeZoneInfoEvaluator(this);
        ExceptionDates = new ExceptionDates(ExceptionDatesPeriodLists);
        RecurrenceDates = new RecurrenceDates(RecurrenceDatesPeriodLists);
    }

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);
        
        Initialize();
    }

    public virtual string? TzId
    {
        get =>
            Parent is not VTimeZone tz
                ? null
                : tz.TzId;
    }

    /// <summary>
    /// Returns the name of the current Time Zone.
    /// <example>
    ///     The following are examples:
    ///     <list type="bullet">
    ///         <item>EST</item>
    ///         <item>EDT</item>
    ///         <item>MST</item>
    ///         <item>MDT</item>
    ///     </list>
    /// </example>
    /// </summary>
    public virtual string? TimeZoneName
    {
        get => TimeZoneNames.Count > 0
            ? TimeZoneNames[0]
            : null;
        set
        {
            TimeZoneNames.Clear();
            TimeZoneNames.Add(value ?? string.Empty);
        }
    }

    public virtual UtcOffset? OffsetFrom
    {
        get => Properties.Get<UtcOffset>("TZOFFSETFROM");
        set => Properties.Set("TZOFFSETFROM", value);
    }

    public virtual UtcOffset? OffsetTo
    {
        get => Properties.Get<UtcOffset>("TZOFFSETTO");
        set => Properties.Set("TZOFFSETTO", value);
    }

    public virtual IList<string> TimeZoneNames
    {
        get => Properties.GetMany<string>("TZNAME");
        set => Properties.Set("TZNAME", value);
    }

    public virtual CalDateTime? DtStart
    {
        get => Start;
        set => Start = value;
    }

    public virtual CalDateTime? Start
    {
        get => Properties.Get<CalDateTime>("DTSTART");
        set => Properties.Set("DTSTART", value);
    }

    internal IList<PeriodList> ExceptionDatesPeriodLists
    {
        get => Properties.GetMany<PeriodList>("EXDATE");
        set => Properties.Set("EXDATE", value);
    }

    public virtual ExceptionDates ExceptionDates { get; private set; } = null!;

    internal IList<PeriodList> RecurrenceDatesPeriodLists
    {
        get => Properties.GetMany<PeriodList>("RDATE");
        set => Properties.Set("RDATE", value);
    }

    public virtual RecurrenceDates RecurrenceDates { get; private set; } = null!;

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

    public IEvaluator? Evaluator => _evaluator;

    public virtual IEnumerable<Occurrence> GetOccurrences(ZonedDateTime startTime, EvaluationOptions? options = null)
        => RecurrenceUtil.GetOccurrences(this, startTime.Zone, startTime.ToInstant(), options);

    public virtual IEnumerable<Occurrence> GetOccurrences(DateTimeZone timeZone, Instant? startTime = null, EvaluationOptions? options = null)
        => RecurrenceUtil.GetOccurrences(this, timeZone, startTime, options);
}
