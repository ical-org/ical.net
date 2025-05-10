//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net.TimeZone;

/// <summary>
/// Represents a custom <see cref="DateTimeZone"/> that can be used to represent a timezone
/// created from a <see cref="VTimeZone"/> or to create any other custom time zone.
/// </summary>
internal class CalDateTimeZone : DateTimeZone   // for the public API we use DateTimeZone
{
    private readonly List<ZoneInterval> _zoneIntervals = new();

    /// <summary>
    /// Creates a new instance of <see cref="CalDateTimeZone"/> with the specified ID and zone intervals.
    /// </summary>
    /// <param name="id">The timezone ID</param>
    /// <param name="zoneIntervals">A list of <see cref="ZoneInterval"/>s, sorted by start date.</param>
    public CalDateTimeZone(string id, ICollection<ZoneInterval> zoneIntervals)
        : base(id, zoneIntervals.Count == 0, GetMinOffset(zoneIntervals), GetMaxOffset(zoneIntervals))
    {
        _zoneIntervals.AddRange(zoneIntervals);
    }

    /// <inheritdoc/>
    public override ZoneLocalMapping MapLocal(LocalDateTime localDateTime)
    {
        // MapLocal calls GetZoneInterval with the localDateTime converted to an Instant
        var result = base.MapLocal(localDateTime);
        return result;
    }

    /// <inheritdoc/>
    public override ZoneInterval GetZoneInterval(Instant instant)
    {
        // instant is a NodaTime internal LocalInstant
        // that is created from LocalDateTime

        // Handle instants before the first zone interval
        if (_zoneIntervals.Count == 0 || instant < _zoneIntervals[0].Start)
            return CreateZoneInterval(
                "LMT", // Before standardized time zones used for "Local Mean Time" :)
                Instant.MinValue, // makes zoneInterval.localStart being equal to StartOfTime
                _zoneIntervals.Count > 0 ? _zoneIntervals[0].Start : Instant.MaxValue,
                Offset.Zero,
                Offset.Zero,
                Id);

        // Binary search for the matching interval
        var low = 0;
        var high = _zoneIntervals.Count - 1;

        while (low <= high)
        {
            var mid = (low + high) / 2;
            var interval = _zoneIntervals[mid];

            if (instant < interval.Start)
                high = mid - 1;
            else if (instant >= interval.End)
                low = mid + 1;
            else
                return interval;
        }

        // If we get here, the instant is *after all defined intervals*
        // Return a default interval (this case shouldn't happen with proper data)
        return CreateZoneInterval(
            Id,
            _zoneIntervals[_zoneIntervals.Count - 1].End,
            Instant.MaxValue,
            _zoneIntervals[_zoneIntervals.Count - 1].WallOffset,
            Offset.Zero);
    }

    private static ZoneInterval CreateZoneInterval(string name, Instant start, Instant end,
                                          Offset wallOffset, Offset savings, string? namePrefix = null)
    {
        return new ZoneInterval(
            namePrefix != null ? $"{namePrefix}_{name}" : name,
            start,
            end,
            wallOffset,
            savings);
    }

    private static Offset GetMinOffset(ICollection<ZoneInterval> zoneIntervals)
        => zoneIntervals.Count > 0 ? zoneIntervals.Min(t => t.WallOffset) : Offset.Zero;

    private static Offset GetMaxOffset(ICollection<ZoneInterval> zoneIntervals)
        => zoneIntervals.Count > 0 ? zoneIntervals.Max(t => t.WallOffset) : Offset.Zero;
}
