//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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
public class RecurrenceId : ICalendarParameterCollectionContainer
{
    /// <summary>
    /// Initializes a new instance with the specified start time and recurrence range.
    /// </summary>
    /// <param name="start">The start time of the recurrence instance.</param>
    /// <param name="range">The recurrence range that defines the scope of the instance.
    /// If <paramref name="range"/> is <see langword="null"/>, the default value
    /// <see cref="RecurrenceRange.ThisInstance"/> is used.</param>
    public RecurrenceId(CalDateTime start, RecurrenceRange? range = null)
    {
        StartTime = start;
        Range = range ?? RecurrenceRange.ThisInstance;
        Parameters = new ParameterList();
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

    public IParameterCollection Parameters { get; set; }
}

/// <summary>
/// The range of recurrence instances that a <see cref="RecurrenceId"/> applies to.
/// </summary>
public enum RecurrenceRange
{
    /// <summary>
    /// The scope is limited to the specific instance identified by
    /// the <see cref="RecurrenceId.StartTime"/>.
    /// </summary>
    ThisInstance,
    /// <summary>
    /// Represents a date range that includes the specified date and all future dates.
    /// </summary>
    ThisAndFuture
}
