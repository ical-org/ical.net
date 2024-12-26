//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

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
        _endTime = end;
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and lasting for the given duration.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="duration"></param>
    /// <exception cref="ArgumentException"></exception>
    public Period(IDateTime start, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentException("Duration must be greater than or equal to zero.", nameof(duration));
        }

        StartTime = start;
        _duration = duration;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not Period p) return;

        StartTime = p.StartTime.Copy<IDateTime>();
        _endTime = p._endTime?.Copy<IDateTime>();
        _duration = p._duration;
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

    private IDateTime? _endTime;

    /// <summary>
    /// Gets either the end time of the period that was set,
    /// or calculates the exact end time based on the nominal duration.
    /// <para/>
    /// Sets the end time of the period.
    /// Either the <see cref="EndTime"/> or the <see cref="Duration"/> can be set at a time.
    /// The last one set will be stored, and the other will be calculated.
    /// </summary>
    public virtual IDateTime? EndTime
    {
        get => _endTime ?? (_duration != null ? StartTime.Add(_duration.Value.ToDuration()) : null);
        set
        {
            _endTime = value;
            if (_endTime != null)
            {
                _duration = null;
            }
        }
    }

    private TimeSpan? _duration;

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
    public virtual TimeSpan? Duration
    {
        get
        {
            if (_endTime == null && !StartTime.HasTime)
            {
                return TimeSpan.FromDays(1);
            }
            return _duration ?? _endTime?.Subtract(StartTime).ToTimeSpan();
        }
        set
        {
            _duration = value;
            if (_duration != null)
            {
                _endTime = null;
            }
        }
    }

    /// <summary>
    /// Gets the original start time, end time, and duration as they were set,
    /// while <see cref="EndTime"/> or <see cref="Duration"/> properties may be calculated.
    /// </summary>
    /// <returns></returns>
    public (IDateTime StartTime, IDateTime? EndTime, TimeSpan? Duration) GetOriginalValues()
    {
        return (StartTime, _endTime, _duration);
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

        // End time is exclusive
        return EndTime == null || EndTime.GreaterThan(dt);
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
    {
        // Check if the start or end time of the given period is within the current period
        var startContained = Contains(period.StartTime);
        var endContained = period.EndTime != null && Contains(period.EndTime);

        // Check if the current period is completely within the given period
        var currentStartContained = period.Contains(StartTime);
        var currentEndContained = EndTime != null && period.Contains(EndTime);

        return startContained || endContained || currentStartContained || currentEndContained;
    }

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

