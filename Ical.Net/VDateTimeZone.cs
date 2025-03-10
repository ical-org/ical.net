#nullable enable
using System.Collections.Generic;
using System.Linq;

using Ical.Net.CalendarComponents;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

/// <summary>
/// A <see cref="DateTimeZone"/> created from a <see cref="VTimeZone"/>.
/// </summary>
internal class VDateTimeZone : DateTimeZone
{
    private readonly List<ZoneInterval> _intervals;

    /// <summary>
    /// Initializes a new instance of the <see cref="VDateTimeZone"/> class.
    /// </summary>
    /// <param name="id">The timezone ID.</param>
    /// <param name="intervals">The list of <see cref="ZoneInterval"/>s.</param>
    public VDateTimeZone(string id, List<ZoneInterval> intervals)
        : base(id, false, intervals.First().WallOffset, intervals.Last().WallOffset)
    {
        _intervals = intervals;
    }

    /// <inheritdoc/>
    public override ZoneInterval GetZoneInterval(Instant instant)
    {
        return _intervals.LastOrDefault(interval =>
                   interval.HasStart && interval.Start <= instant && (!interval.HasEnd || interval.End > instant))
               ?? _intervals.Last();
    }
}
