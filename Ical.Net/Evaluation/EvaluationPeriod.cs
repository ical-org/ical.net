//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using NodaTime;

namespace Ical.Net.Evaluation;
public class EvaluationPeriod : IComparable<EvaluationPeriod>
{
    public ZonedDateTime Start { get; private set; }
    public ZonedDateTime? End { get; private set; }

    public EvaluationPeriod(ZonedDateTime start, ZonedDateTime? end = null)
    {
        Start = start;
        End = end;
    }

    public EvaluationPeriod WithZone(DateTimeZone zone)
    {
        return new(Start.WithZone(zone), End?.WithZone(zone));
    }

    public bool Equals(EvaluationPeriod other)
    {
        return Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is EvaluationPeriod value && Equals(value);
    }

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public int CompareTo(EvaluationPeriod? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Start.ToInstant().CompareTo(other.Start.ToInstant());
    }
}
