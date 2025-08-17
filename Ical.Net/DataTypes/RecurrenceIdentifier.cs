//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents the identifier for a specific instance of a recurring event.
/// </summary>
/// <example>
/// RECURRENCE-ID:20250401T133000Z: The instance for this date only
/// RECURRENCE-ID;RANGE=THISANDFUTURE:20250401T133000Z: This specifies the instance for this date and all future instances.
/// </example>
/// <remarks>
/// This class is used to uniquely identify a particular occurrence within a recurring series. The
/// identifier consists of a date and a recurrence range, which together specify the instance.
/// </remarks>
public class RecurrenceIdentifier : IComparable<RecurrenceIdentifier>
{
    /// <summary>
    /// Initializes a new instance with the specified start time and recurrence range.
    /// </summary>
    /// <param name="start">The start time of the recurrence instance.</param>
    /// <param name="range">The recurrence range that defines the scope of the instance.
    /// If <paramref name="range"/> is <see langword="null"/>, the default value
    /// <see cref="RecurrenceRange.ThisInstance"/> is used.</param>
    public RecurrenceIdentifier(CalDateTime start, RecurrenceRange? range = null)
    {
        StartTime = start;
        Range = range ?? RecurrenceRange.ThisInstance;
    }

    /// <summary>
    /// Gets or sets the start date and time of the specific instance within the recurring series
    /// that this identifier refers to and that should get overridden.
    /// </summary>
    public CalDateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the recurrence range that determines the scope of the recurrence pattern.
    /// </summary>
    public RecurrenceRange Range { get; set; }

    /// <summary>
    /// Compares the current instance with another <see cref="RecurrenceIdentifier"/>
    /// object and returns an integer that indicates whether the current
    /// instance precedes, follows, or occurs in the same position in the
    /// sort order as the other object.
    /// </summary>
    /// <remarks>
    /// The comparison is performed first by the <see cref="StartTime"/> property. If the <see
    /// cref="StartTime"/> values  are equal, the <see cref="Range"/> property is used as a tiebreaker.
    /// </remarks>
    /// <param name="other">
    /// The <see cref="RecurrenceIdentifier"/> to compare with the current instance.
    /// Can be <see langword="null"/>.
    /// </param>
    public int CompareTo(RecurrenceIdentifier? other)
    {
        if (other is null)
        {
            return 1;
        }

        var startComparison = StartTime.ToInstant().CompareTo(other.StartTime.ToInstant());
        if (startComparison != 0)
        {
            return startComparison;
        }

        return Range.CompareTo(other.Range);
    }
}

/// <summary>
/// The range of recurrence instances that a <see cref="RecurrenceIdentifier"/> applies to.
/// </summary>
public enum RecurrenceRange
{
    /// <summary>
    /// The scope is limited to the specific instance identified by
    /// the <see cref="RecurrenceIdentifier.StartTime"/>.
    /// </summary>
    ThisInstance,
    /// <summary>
    /// Represents a date range that includes the specified date and all future dates.
    /// </summary>
    ThisAndFuture
}
