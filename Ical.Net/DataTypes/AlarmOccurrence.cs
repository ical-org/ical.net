//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using NodaTime;

namespace Ical.Net.DataTypes;

/// <summary>
/// A class that represents a specific occurrence of an <see cref="Alarm"/>.
/// </summary>
/// <remarks>
/// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
/// the alarm occurs, the <see cref="Alarm"/> that fired, and the
/// component on which the alarm fired.
/// </remarks>
public class AlarmOccurrence : IComparable<AlarmOccurrence>
{
    public ZonedDateTime Start { get; private set; }

    public IRecurringComponent? Component { get; private set; }

    public Alarm? Alarm { get; private set; }

    public AlarmOccurrence(AlarmOccurrence ao)
    {
        Start = ao.Start;
        Component = ao.Component;
        Alarm = ao.Alarm;
    }

    public AlarmOccurrence(Alarm a, ZonedDateTime start, IRecurringComponent rc)
    {
        Alarm = a;
        Start = start;
        Component = rc;
    }

    public int CompareTo(AlarmOccurrence? other)
    {
        if (other is null) return 1;
        return Start.ToInstant().CompareTo(other.Start.ToInstant());
    }
}
