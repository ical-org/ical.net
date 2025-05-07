//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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

    public override bool Equals(object? obj)
    {
        var tzi = obj as VTimeZoneInfo;
        if (tzi != null)
        {
            return Equals(TimeZoneName, tzi.TimeZoneName) &&
                   Equals(OffsetFrom, tzi.OffsetFrom) &&
                   Equals(OffsetTo, tzi.OffsetTo);
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = TimeZoneName?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (OffsetFrom?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (OffsetTo?.GetHashCode() ?? 0);

            return hashCode;
        }
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

    public virtual CalDateTime? RecurrenceId
    {
        get => Properties.Get<CalDateTime>("RECURRENCE-ID");
        set => Properties.Set("RECURRENCE-ID", value);
    }

    public IEvaluator? Evaluator => _evaluator;

    public virtual IEnumerable<Occurrence> GetOccurrences(CalDateTime? startTime = null, EvaluationOptions? options = null)
        => RecurrenceUtil.GetOccurrences(this, startTime, options);
}
