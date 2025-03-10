// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

/// <summary>
/// Builds a <see cref="DateTimeZone"/> from a <see cref="VTimeZone"/>.
/// </summary>
internal class VTimeZoneBuilder
{
    private readonly string _id;
    private readonly List<ZoneInterval> _intervals = new();

    public VTimeZoneBuilder(VTimeZone vTimeZone)
    {
        var timeZoneInfos = vTimeZone.TimeZoneInfos.OrderBy(tzi => tzi.Start.AsUtc).ToList();
        if (timeZoneInfos.Count == 0) return;

        _id = timeZoneInfos[0].TzId;

        for (var i = 0; i < timeZoneInfos.Count; i++)
        {
            var timeZoneInfo = timeZoneInfos[i];
            var start = Instant.FromDateTimeUtc(timeZoneInfo.Start.AsUtc);
            var end = i < timeZoneInfos.Count - 1
                ? Instant.FromDateTimeUtc(timeZoneInfos[i + 1].Start.AsUtc)
                : Instant.MaxValue;
            var offset = Offset.FromTimeSpan(timeZoneInfo.OffsetTo.Offset);
            var savings = Offset.FromTimeSpan(timeZoneInfo.OffsetTo.Offset - timeZoneInfo.OffsetFrom.Offset);

            var interval = new ZoneInterval(timeZoneInfo.TimeZoneName, start, end, offset, savings);
            _intervals.Add(interval);
        }
    }

    /// <summary>
    /// Builds the <see cref="DateTimeZone"/> and adds it to the <see cref="VTimeZoneProvider"/>.
    /// </summary>
    /// <returns>Returns the built <see cref="DateTimeZone"/>.
    /// </returns>
    public DateTimeZone? Build()
    {
        var provider = VTimeZoneProvider.Instance.Add(_id, _intervals);
        return provider.GetZoneOrNull(_id);
    }
}
