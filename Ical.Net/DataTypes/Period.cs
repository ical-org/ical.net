//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an iCalendar period of time.
/// </summary>
public class Period : EncodableDataType, IComparable<Period>
{
    public Period() { }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and ending at the given time. The latter may be null.
    /// <para/>
    /// A <see cref="Period"/> that has a date-only <see cref="StartTime"/>, no <see cref="EndTime"/>
    /// and no duration set, is considered to last for one day.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Period(IDateTime start, IDateTime? end = null)
    {
        if (end != null && end.LessThanOrEqual(start))
        {
            throw new ArgumentException("End time must be greater than end time.", nameof(end));
        }

        StartTime = start ?? throw new ArgumentNullException(nameof(start));
        EndTime = end;
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and lasting for the given duration.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="duration"></param>
    /// <exception cref="ArgumentException"></exception>
    public Period(IDateTime start, Duration duration)
    {
        if (duration.Sign < 0)
            throw new ArgumentException("Duration must be greater than or equal to zero.", nameof(duration));

        StartTime = start;
        Duration = duration;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not Period p) return;

        StartTime = p.StartTime.Copy<IDateTime>();
        EndTime = p.EndTime?.Copy<IDateTime>();
        Duration = p.Duration;
    }

    protected bool Equals(Period other) => Equals(StartTime, other.StartTime) && Equals(EndTime, other.EndTime) && Duration.Equals(other.Duration);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Period) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = StartTime.GetHashCode();
            hashCode = (hashCode * 397) ^ (EndTime?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ Duration.GetHashCode();
            return hashCode;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var periodSerializer = new PeriodSerializer();
        return periodSerializer.SerializeToString(this);
    }

    /// <summary>
    /// Gets or sets the start time of the period.
    /// </summary>
    public virtual IDateTime StartTime { get; set; } = null!;

    /// <summary>
    /// Gets either the end time of the period that was set,
    /// or calculates the exact end time based on the nominal duration.
    /// <para/>
    /// Sets the end time of the period.
    /// Either the <see cref="EndTime"/> or the <see cref="Duration"/> can be set at a time.
    /// The last one set will be stored, and the other will be calculated.
    /// </summary>
    public virtual IDateTime? EndTime { get; set; }

    /// <summary>
    /// Gets either the nominal duration of the period that was set,
    /// or calculates the exact duration based on the end time.
    /// <para/>
    /// Sets the duration of the period.
    /// Either the <see cref="EndTime"/> or the <see cref="Duration"/> can be set at a time.
    /// The last one set will be stored, and the other will be calculated.
    /// <para/>
    /// A <see cref="Period"/> that has a date-only <see cref="StartTime"/>, no <see cref="EndTime"/>
    /// and no duration set, is considered to last for one day.
    /// </summary>
    public virtual Duration? Duration { get; set; }

    internal Duration GetEffectiveDuration()
    {
        if (Duration is { } d)
            return d;

        if (EndTime is { } endTime)
            return endTime.Subtract(StartTime);

        if (!StartTime.HasTime)
            return DataTypes.Duration.FromDays(1);

        return DataTypes.Duration.Zero;
    }

    internal IDateTime GetEffectiveEndTime()
    {
        if (EndTime is { } endTime)
            return endTime;

        return StartTime.Add(GetEffectiveDuration());
    }

    /// <summary>
    /// Checks if the given date time is contained within the period.
    /// </summary>
    /// <remarks>
    /// The method is timezone-aware.
    /// </remarks>
    /// <param name="dt"></param>
    public virtual bool Contains(IDateTime? dt)
    {
        // Start time is inclusive
        if (dt == null || !StartTime.LessThanOrEqual(dt))
        {
            return false;
        }

        var endTime = GetEffectiveEndTime();
        // End time is exclusive
        return endTime == null || endTime.GreaterThan(dt);
    }

    /// <summary>
    /// Checks if the start time of the given period is contained within the current period
    /// or if the end time of the given period is contained within the current period.
    /// It also checks the case where the given period completely overlaps the current period.
    /// </summary>
    /// <remarks>
    /// The method is timezone-aware.
    /// </remarks>
    /// <param name="period"></param>
    /// <returns></returns>
    public virtual bool CollidesWith(Period period)
            => Contains(period.StartTime)
            || period.Contains(StartTime)
            || Contains(period.GetEffectiveEndTime())
            || period.Contains(GetEffectiveEndTime());

    public int CompareTo(Period? other)
    {
        if (other == null)
        {
            return 1;
        }

        if (StartTime.Equals(other.StartTime))
        {
            return 0;
        }
        if (StartTime.LessThan(other.StartTime))
        {
            return -1;
        }

        // StartTime is greater than other.StartTime
        return 1;
    }
}

