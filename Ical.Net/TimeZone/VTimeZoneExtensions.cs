//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net.TimeZone;

/// <summary>
/// Extensions for the <see cref="VTimeZone"/> class.
/// </summary>
internal static class VTimeZoneExtensions
{
    /// <summary>
    /// Converts a collection of <see cref="VTimeZone"/>s
    /// to a collection of <see cref="CalDateTimeZone"/>s.
    /// </summary>
    /// <param name="vTimeZones"></param>
    /// <returns></returns>
    public static IEnumerable<CalDateTimeZone> ToDateTimeZones(this ICollection<VTimeZone> vTimeZones)
        => vTimeZones.Select(vTimeZone => vTimeZone.ToDateTimeZone());

    /// <summary>
    /// Converts a <see cref="VTimeZone"/> to a <see cref="CalDateTimeZone"/>.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static CalDateTimeZone ToDateTimeZone(this VTimeZone vTimeZone)
    {
        if (TryCreateIntervals(vTimeZone, out var tzId, out var intervals))
            return new CalDateTimeZone(tzId, intervals);

        throw new ArgumentException($"VTimeZone '{vTimeZone.TzId}' does not contain valid TimeZoneInfos.", nameof(vTimeZone));
    }

    /// <summary>
    /// Converts a <see cref="T:System.DateTime" /> of any kind to a <see cref="LocalDateTime"/> in the ISO calendar.
    /// Then, the given <paramref name="offset"/> is applied to the <see cref="LocalDateTime"/> to create an <see cref="Instant"/>.
    /// <para/>
    /// Any <see cref="P:System.DateTime.Kind" /> will create the same day/hour/minute etc.
    /// </summary>
    private static Instant CreateInstant(DateTime dt, Offset offset)
    {
        return LocalDateTime
            .FromDateTime(dt)
            .WithOffset(offset)
            .ToInstant();
    }

    /// <summary>
    /// A temporary class to hold transition information
    /// that can be used to create <see cref="ZoneInterval"/>s.
    /// </summary>
    private sealed class Transition(string tzName, Instant start, Offset offset, Offset savings, Instant? end = null)
    {
        public string TzName { get; } = tzName;
        public Instant Start { get; } = start;

        /// <summary>
        /// Will be set in <see cref="CreateIntervalsFromTransitions"/>
        /// </summary>
        public Instant? End { get; set; } = end;
        public Offset Offset { get; } = offset;
        public Offset Savings { get; } = savings;
    }

    private static bool TryCreateIntervals(VTimeZone vTimeZone, out string tzId,
        out List<ZoneInterval> intervals)
    {
        if (vTimeZone.TzId == null)
            throw new ArgumentNullException(nameof(vTimeZone), "VTimeZone.TzId must not be null");

        var transitions = new SortedList<Instant, Transition>();
        intervals = new List<ZoneInterval>();
        tzId = vTimeZone.TzId;

        var timeZoneInfos = vTimeZone.TimeZoneInfos
            .Where(tzi => tzi.Start != null)
            .OrderBy(tzi => tzi.Start!.Value)
            .ToList();

        if (timeZoneInfos.Count == 0)
            return false;

        foreach (var tzi in timeZoneInfos)
        {
            var isDaylight = tzi.Name?.Equals(Components.Daylight, StringComparison.OrdinalIgnoreCase) == true;
            var tzName = tzi.TimeZoneName ?? (isDaylight ? "DST" : "STD");
            var offsetFrom = tzi.OffsetFrom != null ? Offset.FromTimeSpan(tzi.OffsetFrom.Offset) : Offset.Zero;
            var offsetTo = tzi.OffsetTo != null ? Offset.FromTimeSpan(tzi.OffsetTo.Offset) : Offset.Zero;
            var savings = isDaylight ? (offsetTo - offsetFrom) : Offset.Zero;

            var tzEvaluator = new TimeZoneInfoEvaluator(tzi);
            var start = tzi.Start!; // Start is not null due to the filter above
            var periods = tzEvaluator.Evaluate(start, start, null)
                .TakeWhile(d => d.StartTime.Value.Date.Year < DateOnly.MaxValue.Year);

            foreach (var startTime in periods.Select(period => period.StartTime))
            {
                var instant = CreateInstant(startTime.Value, savings);
                transitions.Add(instant, new Transition(tzName, instant, offsetTo, savings));
            }
        }

        CreateIntervalsFromTransitions(transitions, intervals);

        return true;
    }

    /// <summary>
    /// Updates the end of each transition to the start of the next transition,
    /// and creates a list of <see cref="ZoneInterval"/>s from the transitions.
    /// </summary>
    private static void CreateIntervalsFromTransitions(SortedList<Instant, Transition> transitions,
        List<ZoneInterval> intervals)
    {
        Transition? previousTransition = null;

        foreach (var transition in transitions.Values)
        {
            if (previousTransition != null)
                previousTransition.End = transition.Start;
            previousTransition = transition;
        }

        // Handle the last transition  
        if (previousTransition != null)
            previousTransition.End = Instant.MaxValue;

        // Add all transitions to intervals  
        foreach (var transition in transitions.Values)
            intervals.Add(new ZoneInterval(
                transition.TzName,
                transition.Start,
                transition.End ?? Instant.MaxValue,
                transition.Offset,
                transition.Savings
            ));

        transitions.Clear();
    }
}
