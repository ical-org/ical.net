//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.Serialization.DataTypes;
using NodaTime;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an iCalendar period of time.
/// <para/>
/// A period can be defined<br/>
/// 1. by a start time and an end time,<br/>
/// 2. by a start time and a duration,<br/>
/// 3. by a start date/time or date-only, with the duration unspecified.
/// </summary>
public class Period : EncodableDataType
{
    private CalDateTime _startTime = null!;
    private CalDateTime? _endTime;
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

    // Needed for the serialization factory
    internal Period() { }

    internal Period(Instant start, Instant end)
        : this(new CalDateTime(start), new CalDateTime(end)) { }

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
        // Ensure consistent arguments
        if (end != null && start.TzId != end.TzId)
            throw new ArgumentException($"Start time ({start}) and end time ({end}) must have the same timezone.");

        if (end != null && start.HasTime != end.HasTime)
            throw new ArgumentException(
                $"Start time ({start}) and end time ({end}) must both have a time or both be date-only.");

        if (end != null)
        {
            bool isEndBeforeStart;
            if (end.IsFloating)
            {
                // Both are floating, compare local times
                isEndBeforeStart = end.ToLocalDateTime() < start.ToLocalDateTime();
            }
            else
            {
                // Both are zoned, so compare instants
                isEndBeforeStart = end.ToInstant() < start.ToInstant();
            }

            if (isEndBeforeStart)
                throw new ArgumentException($"End time ({end}) must be greater than or equal to start time ({start}).", nameof(end));
        }

        _startTime = start;
        _endTime = end;
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

        if (!start.HasTime && duration.HasTime)
            throw new ArgumentException($"Exact Duration '{duration}' cannot be added to date-only value '{start}'", nameof(start));

        _startTime = start;
        _duration = duration;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not Period p) return;

        _startTime = p._startTime;
        _endTime = p._endTime;
        _duration = p._duration;
    }

    protected bool Equals(Period other) => Equals(StartTime, other.StartTime) && Equals(EndTime, other.EndTime) && Equals(Duration, other.Duration);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Period) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(StartTime, EndTime, Duration);

    /// <inheritdoc/>
    public override string? ToString()
    {
        var periodSerializer = new PeriodSerializer();
        return periodSerializer.SerializeToString(this);
    }

    /// <summary>
    /// Gets the start time of the period.
    /// </summary>
    public virtual CalDateTime StartTime => _startTime;

    /// <summary>
    /// Gets the original end time that was set,
    /// </summary>
    public virtual CalDateTime? EndTime => _endTime;

    /// <summary>
    /// Gets the original duration of the period as it was set.<br/>
    /// </summary>
    public virtual Duration? Duration => _duration;

    public bool HasEndOrDuration => _duration is not null || _endTime is not null;

    internal string? TzId => _startTime.TzId; // same timezone for start and end

    internal PeriodKind PeriodKind
    {
        get
        {
            if (HasEndOrDuration)
            {
                return PeriodKind.Period;
            }

            return _startTime.HasTime ? PeriodKind.DateTime : PeriodKind.DateOnly;
        }
    }
}
