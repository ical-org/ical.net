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

namespace Ical.Net;

public class VTimeZoneInfo : CalendarComponent, IRecurrable
{
    private TimeZoneInfoEvaluator? _evaluator;

    public VTimeZoneInfo()
    {
        // FIXME: how do we ensure SEQUENCE doesn't get serialized?
        //base.Sequence = null;
        // iCalTimeZoneInfo does not allow sequence numbers
        // Perhaps we should have a custom serializer that fixes this?

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
            !(Parent is VTimeZone tz)
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

    public virtual IList<RecurrencePattern> ExceptionRules
    {
        get => Properties.GetMany<RecurrencePattern>("EXRULE");
        set => Properties.Set("EXRULE", value);
    }

    internal IList<PeriodList> RecurrenceDatesPeriodLists
    {
        get => Properties.GetMany<PeriodList>("RDATE");
        set => Properties.Set("RDATE", value);
    }

    public virtual RecurrenceDates RecurrenceDates { get; private set; } = null!;

    public virtual IList<RecurrencePattern> RecurrenceRules
    {
        get => Properties.GetMany<RecurrencePattern>("RRULE");
        set => Properties.Set("RRULE", value);
    }

    /// <summary>
    /// Gets or sets the recurrence identifier for a specific instance of a recurring event.
    /// </summary>
    /// <remarks>Use <see cref="RecurrenceInstance"/> instead, which
    /// supports the RANGE parameter for recurring events.</remarks>
    [Obsolete("Use RecurrenceInstance instead, which supports the RANGE parameter.")]
    public virtual CalDateTime? RecurrenceId
    {
        get => RecurrenceInstance?.Range == RecurrenceRange.ThisInstance ? RecurrenceInstance.StartTime : null;
        set => RecurrenceInstance = value is null ? null : new RecurrenceId(value, RecurrenceRange.ThisInstance);
    }

    /// <summary>
    /// Gets or sets the recurrence identifier for a specific instance of a recurring event.
    /// <para/>
    /// The <see cref="RecurrenceId.Range"/> sets the scope of the recurrence instance:
    /// With <see cref="RecurrenceRange.ThisInstance"/>, the instance is limited to the specific
    /// occurrence identified by the <see cref="RecurrenceId.StartTime"/>.<br/>
    /// With <see cref="RecurrenceRange.ThisAndFuture"/>, the instance applies to the specified
    /// <see cref="RecurrenceId.StartTime"/> and all future occurrences.
    /// </summary>
    public virtual RecurrenceId? RecurrenceInstance
    {
        get => Properties.Get<RecurrenceId>("RECURRENCE-ID");
        set => Properties.Set("RECURRENCE-ID", value);
    }

    public IEvaluator? Evaluator => _evaluator;

    public virtual IEnumerable<Occurrence> GetOccurrences(CalDateTime? startTime = null, EvaluationOptions? options = null)
        => RecurrenceUtil.GetOccurrences(this, startTime, options);
}
