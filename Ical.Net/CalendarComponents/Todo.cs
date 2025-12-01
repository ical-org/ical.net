//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;

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
        set
        {
            if (string.Equals(Status, value, TodoStatus.Comparison))
            {
                return;
            }

            // Automatically set/unset the Completed time, once the
            // component is fully loaded (When deserializing, it shouldn't
            // automatically set the completed time just because the
            // status was changed).
            if (IsLoaded)
            {
                Completed = string.Equals(value, TodoStatus.Completed, TodoStatus.Comparison)
                    ? CalDateTime.Now
                    : null;
            }

            Properties.Set(TodoStatus.Key, value);
        }
    }

    public Todo()
    {
        Name = TodoStatus.Name;

        _mEvaluator = new TodoEvaluator(this);
    }

    /// <summary>
    /// Use this method to determine if an item has been completed.
    /// This takes into account recurrence items and the previous date
    /// of completion, if any.
    /// <note>
    /// This method evaluates the recurrence pattern for this item
    /// as necessary to ensure all relevant information is taken
    /// into account to give the most accurate result possible.
    /// </note>
    /// </summary>
    /// <returns>True if the item has been completed</returns>
    public virtual bool IsCompleted(ZonedDateTime currDt)
    {
        if (Status == TodoStatus.Completed)
        {
            if (Completed == null)
            {
                return true;
            }

            var completed = Completed.ToZonedDateTime(currDt.Zone);

            if (completed.ToInstant() > currDt.ToInstant())
            {
                return true;
            }

            // Evaluate to the previous occurrence.
            var periods = _mEvaluator.EvaluateToPreviousOccurrence(completed, currDt, options: null);

            return periods.All(p => p.Start.ToInstant() <= completed.ToInstant() || currDt.ToInstant() < p.Start.ToInstant());
        }
        return false;
    }

    /// <summary>
    /// Returns 'True' if the item is Active as of <paramref name="currDt"/>.
    /// An item is Active if it requires action of some sort.
    /// </summary>
    /// <param name="currDt">The date and time to test.</param>
    /// <returns>True if the item is Active as of <paramref name="currDt"/>, False otherwise.</returns>
    public virtual bool IsActive(ZonedDateTime value)
        => (DtStart == null || value.ToInstant() >= DtStart.ToZonedDateTime(value.Zone).ToInstant())
           && (!IsCompleted(value) && !IsCancelled);

    /// <summary>
    /// Returns True if the item was cancelled.
    /// </summary>
    /// <returns>True if the item was cancelled, False otherwise.</returns>
    public virtual bool IsCancelled => string.Equals(Status, TodoStatus.Cancelled, TodoStatus.Comparison);

    public override IEvaluator Evaluator => _mEvaluator;

    protected override void OnDeserializing(StreamingContext context)
    {
        //A necessary evil, for now
        base.OnDeserializing(context);
    }
}
