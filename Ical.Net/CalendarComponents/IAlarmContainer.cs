//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;

namespace Ical.Net.CalendarComponents;

public interface IAlarmContainer
{
    /// <summary>
    /// A list of <see cref="Components.Alarm"/>s for this recurring component.
    /// </summary>
    ICalendarObjectList<Alarm> Alarms { get; }

    /// <summary>
    /// Gets a streaming sequence of <see cref="AlarmOccurrence"/>s for all <see cref="Alarms"/>
    /// on this component, with fire times in the range [<paramref name="startTime"/>, <paramref name="endTime"/>).
    /// </summary>
    /// <param name="timeZone">The time zone used to evaluate component occurrences.</param>
    /// <param name="startTime">Lower bound (inclusive) on alarm fire times, or <c>null</c> for no lower bound.</param>
    /// <param name="endTime">Upper bound (exclusive) on alarm fire times, or <c>null</c> for no upper bound.</param>
    /// <param name="options">Evaluation options, or <c>null</c> for defaults.</param>
    /// <returns>A streaming sequence of <see cref="AlarmOccurrence"/> objects.</returns>
    IEnumerable<AlarmOccurrence> GetAlarmOccurrences(DateTimeZone timeZone, Instant? startTime = null, Instant? endTime = null, EvaluationOptions? options = null);
}
