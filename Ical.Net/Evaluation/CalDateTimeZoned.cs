//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.DataTypes;
using NodaTime;

namespace Ical.Net.Evaluation;

/// <summary>
/// A <see cref="CalDateTime"/> and its evaluated timezone representation.
/// The timezone-specific members can be used only if <see cref="DataTypes.CalDateTime"/>
/// can be evaluated to timezone information.
/// Test for <see cref="HasZone"/> to determine if the timezone information is available.
/// </summary>
internal readonly struct CalDateTimeZoned : IEquatable<CalDateTimeZoned?>, IComparable<CalDateTimeZoned>, IFormattable
{
    public CalDateTimeZoned(CalDateTime calDateTime, ZonedDateTime? zonedDateTime)
    {
        CalDateTime = calDateTime;
        ZonedDateTime = zonedDateTime;
    }

    /// <summary>
    /// Gets the iCalendar date and time associated with the current instance.
    /// </summary>
    public CalDateTime CalDateTime { get; }

    /// <summary>
    /// Gets the date and time information, including the time zone, for the current context.
    /// It is <see langword="null"/> if the <see cref="CalDateTime.TzId"/> is <see langword="null"/>.
    /// </summary>
    internal ZonedDateTime? ZonedDateTime { get; }

    /// <summary>
    /// Gets the current <see cref="CalDateTime.Value"/>.
    /// </summary>
    public DateTime Value => CalDateTime.Value;

    /// <summary>
    /// Gets the UTC representation of the current date and time.
    /// </summary>
    /// <remarks>This property retrieves the UTC equivalent of the date and time stored in the
    /// timezone information. If <see cref="HasZone"/> is <see langword="false"/>, this property will
    /// return <see langword="null"/>.
    /// </remarks>
    public DateTime? Utc => ZonedDateTime?.ToDateTimeUtc();

    /// <summary>
    /// Gets the time offset from UTC for the current timezone.
    /// </summary>
    /// <remarks>This property retrieves the <c>Offset</c> of the date and time stored in the
    /// in timezone information. If <see cref="HasZone"/> is <see langword="false"/>,
    /// this property will return <see cref="TimeSpan.Zero"/>.
    /// </remarks>
    public TimeSpan Offset => ZonedDateTime?.Offset.ToTimeSpan() ?? TimeSpan.Zero;

    /// <summary>
    /// Gets the identifier of the time zone associated with the calendar date and time.
    /// </summary>
    public string? TzId => CalDateTime.TzId;

    /// <summary>
    /// Gets a value indicating whether the associated date-time includes a time component.
    /// </summary>
    public bool HasTime => CalDateTime.HasTime;

    /// <summary>
    /// Gets a value indicating whether the current instance has an associated time zone.
    /// </summary>
    public bool HasZone => ZonedDateTime is not null;

    /// <summary>
    /// Gets a value indicating whether the associated calendar date and time is floating.
    /// </summary>
    public bool IsFloating => CalDateTime.IsFloating;

    /// <inheritdoc/>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string that includes the date and time with the timezone identifier,
    /// formatted according to the specified format and culture information.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => CalDateTimeEvaluator.ToString(this, format, formatProvider);

    #region *** Equality ***

    /// <inheritdoc/>
    public bool Equals(CalDateTimeZoned? other)
    {
        return CalDateTime.Equals(other?.CalDateTime) &&
               ZonedDateTime.Equals(other?.ZonedDateTime);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is CalDateTimeZoned other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(CalDateTime, ZonedDateTime);

    #endregion

    #region *** Comparison ***

    /// <inheritdoc/>
    public int CompareTo(CalDateTimeZoned other)
    {
        if (Utc.HasValue && other.Utc.HasValue)
        {
            return Utc.Value.CompareTo(other.Utc.Value);
        }

        throw new InvalidOperationException("Cannot compare instances without timezone as UTC.");
    }

    /// <summary>
    /// Determines whether the current instance represents a point
    /// in time earlier than the specified <see cref="CalDateTimeZoned"/>.
    /// </summary>
    public bool LessThan(CalDateTimeZoned? zoned)
    {
        if (!zoned.HasValue) return false;

        var self = CalDateTime;
        var other = zoned.Value.CalDateTime;

        // CalDateTime.Floating has no timezone information, so we compare the values
        return
            (self.IsFloating || other.IsFloating || self.TzId == other.TzId)
                ? self.Value < other.Value
                : Utc!.Value < zoned.Value.Utc!.Value; // Both have timezones, so UTC is not null
    }

    /// <summary>
    /// Determines whether the current instance represents a point
    /// in time earlier than or equal to the specified <see cref="CalDateTimeZoned"/>.
    /// </summary>
    public bool LessThanOrEqual(CalDateTimeZoned? zoned)
    {
        if (!zoned.HasValue) return false;

        var self = CalDateTime;
        var other = zoned.Value.CalDateTime;

        // CalDateTime.Floating has no timezone information, so we compare the values
        return
            (self.IsFloating || other.IsFloating || self.TzId == other.TzId)
                ? self.Value <= other.Value
                : Utc!.Value <= zoned.Value.Utc!.Value; // Both have timezones, so UTC is not null
    }

    /// <summary>
    /// Determines whether the current instance represents a point
    /// in time later than the specified <see cref="CalDateTimeZoned"/>.
    /// </summary>
    public bool GreaterThan(CalDateTimeZoned? zoned)
    {
        if (!zoned.HasValue) return false;

        var self = CalDateTime;
        var other = zoned.Value.CalDateTime;

        // CalDateTime.Floating has no timezone information, so we compare the values
        return
            (self.IsFloating || other.IsFloating || self.TzId == other.TzId)
                ? self.Value > other.Value
                : Utc!.Value > zoned.Value.Utc!.Value; // Both have timezones, so UTC is not null
    }

    /// <summary>
    /// Determines whether the current instance represents a point
    /// in time later than or equal to the specified <see cref="CalDateTimeZoned"/>.
    /// </summary>
    public bool GreaterThanOrEqual(CalDateTimeZoned? zoned)
    {
        if (!zoned.HasValue) return false;

        var self = CalDateTime;
        var other = zoned.Value.CalDateTime;

        // CalDateTime.Floating has no timezone information, so we compare the values
        return
            (self.IsFloating || other.IsFloating || self.TzId == other.TzId)
                ? self.Value >= other.Value
                : Utc!.Value >= zoned.Value.Utc!.Value; // Both have timezones, so UTC is not null
    }

    #endregion

    #region *** Operators ***

    public static bool operator ==(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return left?.Equals(right) == true;
    }

    public static bool operator !=(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return left?.LessThan(right) == true;
    }

    public static bool operator <=(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return left?.LessThanOrEqual(right) == true;
    }

    public static bool operator >(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return left?.GreaterThan(right) == true;
    }

    public static bool operator >=(CalDateTimeZoned? left, CalDateTimeZoned? right)
    {
        return left?.GreaterThanOrEqual(right) == true;
    }

    #endregion
}
