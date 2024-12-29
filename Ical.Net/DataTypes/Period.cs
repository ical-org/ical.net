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
/// <para/>
/// A period can be defined<br/>
/// 1. by a start time and an end time,<br/>
/// 2. by a start time and a duration,<br/>
/// 3. by a start time only, with the duration unspecified.
/// </summary>
public class Period : EncodableDataType, IComparable<Period>
{
    private IDateTime _startTime = null!;
    private IDateTime? _endTime;
    private Duration? _duration;

    // Needed for the serialization factory
    internal Period() { }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and ending at the given time.
    /// <para/>
    /// If <paramref name="end"/> time is not provided, the period will be considered as starting at the given time,
    /// while the duration is unspecified.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Period(IDateTime start, IDateTime? end = null)
    {
        if (end != null && end.LessThanOrEqual(start))
        {
            throw new ArgumentException("End time must be greater than start time.", nameof(end));
        }
        
        if (end?.TzId != null && start.TzId != end.TzId) throw new ArgumentException("Start and end time must have the same timezone.", nameof(end));
        _startTime = start ?? throw new ArgumentNullException(nameof(start));
        _endTime = end;
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and lasting for the given duration.
    /// <para/>
    /// If <paramref name="duration"/> is not provided, the period will be considered as starting at the given time,
    /// while the duration is unspecified.
    /// <para/>
    /// For a <see cref="Period"/> that lasts full days, add a date-only <see cref="StartTime"/>,
    /// and a <see cref="Duration"/> of <see cref="Duration.FromDays"/> with the number of days.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="duration"></param>
    /// <exception cref="ArgumentException"></exception>
    public Period(IDateTime start, Duration duration)
    {
        if (duration.Sign < 0)
            throw new ArgumentException("Duration must be greater than or equal to zero.", nameof(duration));

        _startTime = start;
        _duration = duration;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not Period p) return;

        _startTime = p._startTime.Copy<IDateTime>();
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
    public override string? ToString()
    {
        var periodSerializer = new PeriodSerializer();
        return periodSerializer.SerializeToString(this);
    }

    /// <summary>
    /// Gets or sets the start time of the period.
    /// </summary>
    public virtual IDateTime StartTime //NOSONAR
    {
        get => _startTime;
        set => _startTime = value;
    }

    /// <summary>
    /// Gets either the end time of the period that was set,
    /// or calculates the exact end time based on the nominal duration.
    /// <para/>
    /// Sets the end time of the period.
    /// Either the <see cref="EndTime"/> or the <see cref="Duration"/> can be set at a time.
    /// The last one set with a value not null will prevail, while the other will become <see langword="null"/>.
    /// </summary>
    public virtual IDateTime? EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            if (_endTime != null)
            {
                _duration = null;
            }
        }
    }

    /// <summary>
    /// Gets the nominal duration of the period that was set, or - if this is <see langword="null"/> -
    /// calculates the exact duration based on the duration.
    /// </summary>
    public virtual IDateTime? EffectiveEndTime => _endTime ?? (_duration != null ? GetEffectiveEndTime() : null);

    /// <summary>
    /// Gets the original duration of the period as it was set.<br/>
    /// See also <seealso cref="EffectiveDuration"/>.
    /// <para/>
    /// Sets the duration of the period.
    /// Either the <see cref="EndTime"/> or the <see cref="Duration"/> can be set at a time.
    /// The last one set with a value not null will prevail, while the other will become <see langword="null"/>.
    /// </summary>
    public virtual Duration? Duration
    {
        get => _duration;
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
    /// Gets the duration of the period that was set, or - if this is <see langword="null"/> -
    /// calculates the exact duration based on the end time.
    /// </summary>
    public virtual Duration? EffectiveDuration => _duration ?? (_endTime != null ? GetEffectiveDuration() : null);

    private Duration GetEffectiveDuration()
    {
        if (_duration is { } d)
            return d;

        if (_endTime is { } endTime)
            return endTime.Subtract(_startTime);

        return DataTypes.Duration.Zero;
    }

    private IDateTime GetEffectiveEndTime()
    {
        if (_endTime is { } endTime)
            return endTime;

        return _startTime.Add(GetEffectiveDuration());
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
        if (dt == null || !_startTime.LessThanOrEqual(dt))
        {
            return false;
        }

        var endTime = GetEffectiveEndTime();
        // End time is exclusive
        return endTime.GreaterThan(dt);
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

    /// <inheritdoc/>
    public int CompareTo(Period? other)
    {
        if (other == null)
        {
            return 1;
        }

        if (StartTime.AsUtc.Equals(other.StartTime.AsUtc))
        {
            return 0;
        }
        if (StartTime.AsUtc <= other.StartTime.AsUtc)
        {
            return -1;
        }

        // StartTime is greater than other.StartTime
        return 1;
    }
}
