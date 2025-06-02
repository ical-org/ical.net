//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an iCalendar period of time.
/// <para/>
/// A period can be defined<br/>
/// 1. by a start time and an end time,<br/>
/// 2. by a start time and a duration,<br/>
/// 3. by a start date/time or date-only, with the duration unspecified.
/// </summary>
public class Period : EncodableDataType, IComparable<Period>
{
    private CalDateTimeZoned _start;
    private CalDateTimeZoned? _end;
    private Duration? _duration;

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time.
    /// It ensures the that a consistent logic is applied to the <paramref name="end"/> and <paramref name="duration"/> parameters.
    /// TLDR; If <paramref name="end"/> is provided, it will be used as the end time of the period.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="duration"></param>
    /// <param name="associatedObject"></param>
    /// <returns>
    /// A new <see cref="Period"/> object if the <paramref name="start"/> parameter is not <see langword="null"/>.
    /// <para/>
    /// If the <paramref name="start"/> parameter is <see langword="null"/>, the method returns <see langword="null"/> immediately.
    /// <para/>
    /// If the <paramref name="end"/> parameter is not <see langword="null"/>, a new <see cref="Period"/> object is created using the <paramref name="start"/> and <paramref name="end"/> times.
    /// <para/>
    /// If the <paramref name="end"/> parameter is <see langword="null"/>, the <paramref name="duration"/> parameter is used to create a new <see cref="Period"/> object using the <paramref name="start"/> time and the specified <paramref name="duration"/>.
    /// <para/>
    /// If both <paramref name="end"/> and <paramref name="duration"/> are <see langword="null"/>, a new <see cref="Period"/> object is created using only the <paramref name="start"/> time.
    /// </returns>
    internal static Period Create(CalDateTime start, CalDateTime? end = null, Duration? duration = null, ICalendarObject? associatedObject = null)
    {
        if (end is not null)
            return new Period(start, end) { AssociatedObject = associatedObject };

        if (duration is not null)
            return new Period(start, duration.Value) { AssociatedObject = associatedObject };
 
        return new Period(start) { AssociatedObject = associatedObject };
    }

    internal Period()
    {
        // Needed for the serialization factory
    }

    internal Period(CalDateTimeZoned start, CalDateTimeZoned? end = null)
    {
        // Ensure consistent arguments
        if (end != null && start.TzId != end.Value.TzId)
            throw new ArgumentException($"Start time ({start}) and end time ({end}) must have the same timezone.");

        if (end != null && start.HasTime != end.Value.HasTime)
            throw new ArgumentException(
                $"Start time ({start}) and end time ({end}) must both have a time or both be date-only.");

        if (end != null && end.Value.ToTimeZone(CalDateTime.UtcTzId)
                .LessThan(start.ToTimeZone(CalDateTime.UtcTzId)))
            throw new ArgumentException($"End time ({end}) must be greater than start time ({start}).", nameof(end));

        _start = start;
        _end = end;
    }

    internal Period(CalDateTimeZoned start, Duration duration)
    {
        if (duration.Sign < 0)
            throw new ArgumentException($"Duration ({duration}) must be greater than or equal to zero.", nameof(duration));

        _start = start;
        _duration = duration;
    }

    internal CalDateTimeZoned Start => _start;

    internal CalDateTimeZoned? End => _end;

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
    public Period(CalDateTime start, CalDateTime? end = null)
    {
        _start = start.AsZoned();
        _end = end?.AsZoned();

        // Ensure consistent arguments
        if (end != null && start.TzId != end.TzId)
            throw new ArgumentException($"Start time ({start}) and end time ({end}) must have the same timezone.");

        if (end != null && start.HasTime != end.HasTime)
            throw new ArgumentException(
                $"Start time ({start}) and end time ({end}) must both have a time or both be date-only.");

        if (end != null && end.AsZoned().ToTimeZone(CalDateTime.UtcTzId)
                .LessThan(start.AsZoned().ToTimeZone(CalDateTime.UtcTzId)))
            throw new ArgumentException($"End time ({end}) as UTC must be greater than start time as UTC ({start}).", nameof(end));
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance starting at the given time
    /// and lasting for the given duration.
    /// <para/>
    /// If <paramref name="duration"/> is not provided, the period will be considered as starting at the given time,
    /// while the duration is unspecified.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="duration"></param>
    /// <exception cref="ArgumentException"></exception>
    public Period(CalDateTime start, Duration duration)
    {
        if (duration.Sign < 0)
            throw new ArgumentException($"Duration ({duration}) must be greater than or equal to zero.", nameof(duration));

        _start = start.AsZoned();
        _duration = duration;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not Period p) return;

        _start = p._start;
        _end = p._end;
        _duration = p._duration;
    }

    protected bool Equals(Period other) => Equals(Start, other.Start) && Equals(End, other.End) && Equals(Duration, other.Duration);

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
    /// Gets the start time of the period.
    /// </summary>
    public virtual CalDateTime StartTime => _start.CalDateTime;

    /// <summary>
    /// Gets the original end time that was set,
    /// </summary>
    public virtual CalDateTime? EndTime => _end?.CalDateTime;

    /// <summary>
    /// Gets the end time of the period that was set, or - if this is <see langword="null"/> -
    /// calculates the end time based by adding <see cref="EffectiveDuration"/> to the <see cref="StartTime"/>.
    /// If <see cref="Duration"/> and <see cref="EndTime"/> are both <see langword="null"/>, the method returns <see langword="null"/>.
    /// </summary>
    public virtual CalDateTime? EffectiveEndTime
        => EffectiveEnd?.CalDateTime;

    internal CalDateTimeZoned? EffectiveEnd
    {
        get
        {
            var effectiveDuration = EffectiveDuration;

            return _end switch
            {
                null when _duration is null => null,
                { } endTime => endTime,
                // When _duration is not null, effectiveDuration is guaranteed to be not null 
                _ => _start.Add(effectiveDuration!.Value)
            };
        }
    }

    /// <summary>
    /// Gets the original duration of the period as it was set.<br/>
    /// See also <seealso cref="EffectiveDuration"/>.
    /// </summary>
    public virtual Duration? Duration => _duration;

    /// <summary>
    /// Gets the duration of the period that was set, or - if this is <see langword="null"/> -
    /// calculates the exact duration by subtracting <see cref="StartTime"/> from <see cref="EndTime"/>.
    /// If <see cref="Duration"/> and <see cref="EndTime"/> are both <see langword="null"/>, the method returns <see langword="null"/>.
    /// </summary>
    public virtual Duration? EffectiveDuration
    {
        get =>
            _duration switch
            {
                null when _end is null => null,
                { } d => d,
                // _end and _start are not null
                _ => _end.Value.Subtract(_start)
            };
    }

    internal string? TzId => _start.TzId; // same timezone for start and end

    internal PeriodKind PeriodKind
    {
        get
        {
            if (EffectiveDuration != null)
            {
                return PeriodKind.Period;
            }

            return StartTime.HasTime ? PeriodKind.DateTime : PeriodKind.DateOnly;
        }
    }

    /// <summary>
    /// Checks if the given date time is contained within the period.
    /// </summary>
    /// <remarks>
    /// The method is timezone-aware.
    /// </remarks>
    /// <param name="dt"></param>
    public virtual bool Contains(CalDateTime? dt)
        => Contains(dt?.AsZoned());

    internal bool Contains(CalDateTimeZoned? dt)
    {
        // Start time is inclusive
        if (dt == null || !_start.LessThanOrEqual(dt))
        {
            return false;
        }

        var end = EffectiveEnd;
        // End time is exclusive
        return end?.GreaterThan(dt) != false;
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
            => Contains(period.Start)
            || period.Contains(Start)
            || Contains(period.EffectiveEnd)
            || period.Contains(EffectiveEnd);

    /// <inheritdoc/>
    public int CompareTo(Period? other)
    {
        if (other == null)
        {
            return 1;
        }

        if (Start.ToTimeZone(CalDateTime.UtcTzId).Equals(other.Start.ToTimeZone(CalDateTime.UtcTzId)))
        {
            return 0;
        }
        if (Start.LessThanOrEqual(other.Start))
        {
            return -1;
        }

        // Start is greater than other.Start
        return 1;
    }
}
