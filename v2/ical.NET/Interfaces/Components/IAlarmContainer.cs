﻿using System.Collections.Generic;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.General;

namespace ical.net.Interfaces.Components
{
    public interface IAlarmContainer
    {
        /// <summary>
        /// A list of <see cref="Components.Alarm"/>s for this recurring component.
        /// </summary>
        ICalendarObjectList<Alarm> Alarms { get; }

        ///  <summary>
        ///  Polls <see cref="Alarm"/>s for occurrences within the <see cref="Evaluate"/>d
        ///  time frame of this <see cref="RecurringComponent"/>.  For each evaluated
        ///  occurrence if this component, each <see cref="Alarm"/> is polled for its
        ///  corresponding alarm occurrences.
        ///  </summary>
        /// <param name="startTime">The earliest allowable alarm occurrence to poll, or <c>null</c>.</param>
        /// <param name="endTime"></param>
        /// <returns>A List of <see cref="AlarmOccurrence"/> objects, one for each occurrence of the <see cref="Alarm"/>.</returns>
        IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime);
    }
}