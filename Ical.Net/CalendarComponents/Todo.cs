//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// A class that represents an RFC 5545 VTODO component.
/// </summary>
[DebuggerDisplay("{Summary} - {Status}")]
public class Todo : RecurringComponent, IAlarmContainer
{
    private readonly TodoEvaluator _mEvaluator;

    /// <summary>
    /// The date/time when the item was completed.
    /// </summary>
    public virtual CalDateTime? Completed
    {
        get => Properties.Get<CalDateTime>("COMPLETED");
        set => Properties.Set("COMPLETED", value);
    }

    /// <summary>
    /// The due date of the item.
    /// </summary>
    public virtual CalDateTime? Due
    {
        get => Properties.Get<CalDateTime>("DUE");
        set
        {
            Properties.Set("DUE", value);
        }
    }

    /// <summary>
    /// The duration of the item.
    /// </summary>
    // NOTE: Duration is not supported by all systems,
    // (i.e. iPhone) and cannot co-exist with Due.
    // RFC 5545 states:
    //
    //      ; either 'due' or 'duration' may appear in
    //      ; a 'todoprop', but 'due' and 'duration'
    //      ; MUST NOT occur in the same 'todoprop'
    //
    // Therefore, Duration is not serialized, as Due
    // should always be extrapolated from the duration.
    public virtual DataTypes.Duration? Duration
    {
        get => Properties.Get<DataTypes.Duration?>("DURATION");
        set
        {
            Properties.Set("DURATION", value);
        }
    }

    public virtual GeographicLocation? GeographicLocation
    {
        get => Properties.Get<GeographicLocation>("GEO");
        set => Properties.Set("GEO", value);
    }

    public virtual string? Location
    {
        get => Properties.Get<string>("LOCATION");
        set => Properties.Set("LOCATION", value);
    }

    public virtual int PercentComplete
    {
        get => Properties.Get<int>("PERCENT-COMPLETE");
        set => Properties.Set("PERCENT-COMPLETE", value);
    }

    public virtual IList<string> Resources
    {
        get => Properties.GetMany<string>("RESOURCES");
        set => Properties.Set("RESOURCES", value);
    }

    /// <summary>
    /// The status of the item.
    /// </summary>
    public virtual string? Status
    {
        get => Properties.Get<string>(TodoStatus.Key);
        set => Properties.Set(TodoStatus.Key, value);
    }

    public Todo()
    {
        Name = TodoStatus.Name;

        _mEvaluator = new TodoEvaluator(this);
    }

    /// <summary>
    /// Is <c>true</c> if the item is completed. An item is
    /// completed if <c>Status</c> is set to completed,
    /// <c>Completed</c> has a value, or
    /// <c>PercentComplete</c> is 100%.
    /// </summary>
    public virtual bool IsCompleted => Status == TodoStatus.Completed
        || Completed != null
        || PercentComplete == 100;

    /// <summary>
    /// Is <c>true</c> if the item not completed or cancelled.
    /// </summary>
    public virtual bool IsActive => !IsCompleted && !IsCancelled;

    /// <summary>
    /// Is <c>true</c> if item <c>Status</c> is set to cancelled.
    /// </summary>
    public virtual bool IsCancelled => string.Equals(Status, TodoStatus.Cancelled, TodoStatus.Comparison);

    public override IEvaluator Evaluator => _mEvaluator;

    protected override void OnDeserializing(StreamingContext context)
    {
        //A necessary evil, for now
        base.OnDeserializing(context);
    }
}
