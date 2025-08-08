//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// A class that represents an RFC 2445 VALARM component.
/// FIXME: move GetOccurrences() logic into an AlarmEvaluator.
/// </summary>
public class Alarm : CalendarComponent
{
    public virtual string? Action
    {
        get => Properties.Get<string>(AlarmAction.Key);
        set => Properties.Set(AlarmAction.Key, value);
    }

    public virtual Attachment? Attachment
    {
        get => Properties.Get<Attachment>("ATTACH");
        set => Properties.Set("ATTACH", value);
    }

    public virtual IList<Attendee> Attendees
    {
        get => Properties.GetMany<Attendee>("ATTENDEE");
        set => Properties.Set("ATTENDEE", value);
    }

    public virtual string? Description
    {
        get => Properties.Get<string>("DESCRIPTION");
        set => Properties.Set("DESCRIPTION", value);
    }

    public virtual DataTypes.Duration? Duration
    {
        get => Properties.Get<DataTypes.Duration>("DURATION");
        set => Properties.Set("DURATION", value);
    }

    public virtual int Repeat
    {
        get => Properties.Get<int>("REPEAT");
        set => Properties.Set("REPEAT", value);
    }

    public virtual string? Summary
    {
        get => Properties.Get<string>("SUMMARY");
        set => Properties.Set("SUMMARY", value);
    }

    public virtual Trigger? Trigger
    {
        get => Properties.Get<Trigger>(TriggerRelation.Key);
        set => Properties.Set(TriggerRelation.Key, value);
    }

    public Alarm()
    {
        Name = Components.Alarm;
    }

    /// <summary>
    /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
    /// that occur at or after <paramref name="fromDate"/>.
    /// </summary>
    public virtual IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, DateTimeZone timeZone, Instant? fromDate, EvaluationOptions? options)
    {
        if (Trigger == null)
        {
            return [];
        }

        var occurrences = new List<AlarmOccurrence>();

        // If the trigger is relative, it can recur right along with
        // the recurring items, otherwise, it happens once and
        // only once (at a precise time).
        if (Trigger.IsRelative)
        {
            // Ensure that "FromDate" has already been set
            if (fromDate == null)
            {
                fromDate = rc.Start?.ToZonedDateTime(timeZone).ToInstant();
            }

            var triggerDuration = Trigger.Duration!.Value.ToPeriod();

            foreach (var o in rc.GetOccurrences(timeZone, fromDate, options))
            {
                var dt = o.Start;
                if (string.Equals(Trigger.Related, TriggerRelation.End, TriggerRelation.Comparison))
                {
                    dt = o.End;
                }

                var triggerStart = dt.LocalDateTime
                    .Plus(triggerDuration)
                    .InZoneLeniently(dt.Zone);

                occurrences.Add(new AlarmOccurrence(this, triggerStart, rc));
            }
        }
        else
        {
            var dt = Trigger?.DateTime?.Copy().ToZonedDateTime();
            if (dt != null)
            {
                occurrences.Add(new AlarmOccurrence(this, dt.Value, rc));
            }
        }

        // If a REPEAT and DURATION value were specified,
        // then handle those repetitions here.
        AddRepeatedItems(occurrences);

        return occurrences;
    }

    /// <summary>
    /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
    /// since the provided <paramref name="start"/> date/time.  If <paramref name="start"/>
    /// is null, all triggered alarms will be returned.
    /// </summary>
    /// <param name="start">The earliest date/time to poll triggered alarms for.</param>
    /// <param name="options"></param>
    /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
    public virtual IList<AlarmOccurrence> Poll(DateTimeZone timeZone, Instant? start, EvaluationOptions? options = null)
    {
        var results = new List<AlarmOccurrence>();

        // Evaluate the alarms to determine the recurrences
        if (Parent is not RecurringComponent rc)
        {
            return results;
        }

        results.AddRange(GetOccurrences(rc, timeZone, start, options));
        return results;
    }

    /// <summary>
    /// Handles the repetitions that occur from the <c>REPEAT</c> and
    /// <c>DURATION</c> properties.  Each recurrence of the alarm will
    /// have its own set of generated repetitions.
    /// </summary>
    private void AddRepeatedItems(List<AlarmOccurrence> occurrences)
    {
        var len = occurrences.Count;
        for (var i = 0; i < len; i++)
        {
            var ao = occurrences[i];
            if (ao.Component == null)
            {
                continue;
            }

            var alarmTime = ao.Start;
            var duration = Duration?.ToPeriod();

            for (var j = 0; j < Repeat; j++)
            {
                if (duration != null)
                {
                    alarmTime = alarmTime.LocalDateTime
                        .Plus(duration)
                        .InZoneLeniently(alarmTime.Zone);
                }

                occurrences.Add(new AlarmOccurrence(this, alarmTime, ao.Component));
            }
        }
    }
}
