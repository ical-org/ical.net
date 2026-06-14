//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;
using NodaTime;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// A class that represents an RFC 2445 VALARM component.
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
    /// Yields the base fire time followed by any additional fire times produced by
    /// <c>REPEAT</c> and <c>DURATION</c>.
    /// </summary>
    internal IEnumerable<ZonedDateTime> GetFireTimes(ZonedDateTime baseFireTime)
    {
        yield return baseFireTime;
        if (Repeat <= 0 || !Properties.ContainsKey("DURATION")) yield break;
        var period = Duration!.Value.ToPeriod();
        var t = baseFireTime;
        for (var i = 0; i < Repeat; i++)
        {
            t = t.LocalDateTime.Plus(period).InZoneLeniently(t.Zone);
            yield return t;
        }
    }
}
