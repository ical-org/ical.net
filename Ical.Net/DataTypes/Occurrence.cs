//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using Ical.Net.Evaluation;
using Ical.Net.Utility;
using NodaTime;

namespace Ical.Net.DataTypes;

public class Occurrence : IComparable<Occurrence>
{
    public IRecurrable Source { get; private set; }
    public ZonedDateTime Start { get; private set; }
    public ZonedDateTime End { get; private set; }

    public (ZonedDateTime Start, ZonedDateTime End) Period => (Start, End);

    public Occurrence(Occurrence ao)
    {
        Source = ao.Source;
        Start = ao.Start;
        End = ao.End;
    }

    public Occurrence(IRecurrable recurrable, ZonedDateTime start, ZonedDateTime end)
    {
        Source = recurrable;
        Start = start;
        End = end;
    }

    public CalDateTime? DtStart
    {
        get
        {
            if (Source.Start is not { } dtStart)
            {
                return null;
            }

            var start = Start;
            var tzid = dtStart.TimeZoneName;

            // Switch to recurrable time zone, if any
            if (tzid is not null)
            {
                start = start.WithZone(DateUtil.GetZone(tzid));
            }

            return dtStart.HasTime
                ? new(start.LocalDateTime, tzid)
                : new(start.Date, tzid);
        }
    }

    /// <summary>
    /// Checks if the given date time is contained within the period.
    /// </summary>
    public bool Contains(CalDateTime? value)
    {
        if (value is null)
        {
            return false;
        }

        // Use floating time from the occurrence's time zone
        var valueInstant = value
            .ToZonedDateTime(Start.Zone)
            .ToInstant();

        return Start.ToInstant() <= valueInstant
            && End.ToInstant() > valueInstant;
    }

    public bool Equals(Occurrence other)
    {
        return Start == other.Start
            && End == other.End
            && Source.Equals(other.Source);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is Occurrence occurrence && Equals(occurrence);
    }

    public override int GetHashCode() => HashCode.Combine(Start, End, Source);

    public override string ToString() => $"Occurrence {Source.GetType().Name} ({Start})";

    public int CompareTo(Occurrence? other)
    {
        if (other is null)
        {
            return 1;
        }

        // Order occurrences ONLY by instant, not time zone.
        return Start.ToInstant().CompareTo(other.Start.ToInstant());
    }
}
