//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using NodaTime;

namespace Ical.Net.DataTypes;

public class FreeBusyEntry : Period
{
    public virtual FreeBusyStatus Status { get; set; }

    public FreeBusyEntry()
    {
        Status = FreeBusyStatus.Busy;
    }

    public FreeBusyEntry(Period period, FreeBusyStatus status)
    {
        // Sets the status associated with a given period, which requires copying the period values
        // Probably the Period object should just have a FreeBusyStatus directly?
        CopyFrom(period);
        Status = status;

        if (EndTime is null && Duration is null)
        {
            throw new ArgumentException("Period must have a duration", nameof(period));
        }
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is FreeBusyEntry fb)
        {
            Status = fb.Status;
        }
    }

    /// <summary>
    /// For backwards compatibility. Value must be in UTC time.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public bool Contains(CalDateTime? dt)
    {
        return dt is null ? false : Contains(dt.ToInstant());
    }

    /// <summary>
    /// Checks if the given instant is contained within the period.
    /// This assumes that all CalDateTime values are in UTC time
    /// according to the spec.
    /// </summary>
    public bool Contains(Instant value)
    {
        var startInstant = StartTime.ToInstant();
        if (startInstant > value)
        {
            return false;
        }

        Instant end;
        if (EndTime is { } endTime)
        {
            end = endTime.ToInstant();
        }
        else if (Duration is { } duration)
        {
            // Convert directly from period to duration
            // because this is in UTC which always has
            // the same days and weeks.
            end = startInstant.Plus(duration.ToPeriod().ToDuration());
        }
        else
        {
            return false;
        }

        return end > value;
    }

    /// <summary>
    /// CHecks if the given period is within the FREEBUSY entry.
    /// </summary>
    /// <param name="period"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool CollidesWith(Period? period)
    {
        if (period is null)
        {
            return false;
        }

        if (period.StartTime.IsFloating)
        {
            throw new ArgumentException("Period start time must be in UTC");
        }

        var start = StartTime.ToInstant();
        Instant end;
        if (EndTime is { } endTime)
        {
            end = endTime.ToInstant();
        }
        else if (Duration is { } duration)
        {
            // Convert directly from period to duration
            // because this is in UTC which always has
            // the same days and weeks.
            end = start.Plus(duration.ToPeriod().ToDuration());
        }
        else
        {
            return false;
        }

        var otherStart = period.StartTime.ToInstant();
        Instant otherEnd;
        if (period.EndTime is { } periodEndTime)
        {
            if (periodEndTime.IsFloating)
            {
                throw new ArgumentException("Period end time must be in UTC");
            }

            otherEnd = periodEndTime.ToInstant();
        }
        else if (period.Duration is { } periodDuration)
        {
            // Convert directly from period to duration
            // because this is in UTC which always has
            // the same days and weeks.
            otherEnd = otherStart.Plus(periodDuration.ToPeriod().ToDuration());
        }
        else
        {
            throw new ArgumentException("Period must have a end time or duration");
        }

        return start < otherEnd && otherStart < end;
    }
}
