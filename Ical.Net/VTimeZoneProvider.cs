// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ical.Net.CalendarComponents;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

/// <summary>
/// Provides a custom <see cref="DateTimeZone"/> for a given TZID created from a <see cref="CalendarComponents.VTimeZone"/>
/// </summary>
public class VTimeZoneProvider : IDateTimeZoneProvider
{
    private readonly Dictionary<string, DateTimeZone> _customTimeZones;

    /// <summary>
    /// Creates a <see cref="VTimeZoneProvider"/> from a <see cref="Calendar"/>.
    /// </summary>
    /// <param name="calendar"></param>
    /// <returns></returns>
    public static VTimeZoneProvider FromCalendar(Calendar calendar) => FromTimeZone(calendar.TimeZones);

    /// <summary>
    /// Creates a <see cref="VTimeZoneProvider"/> from a collection of <see cref="VTimeZone"/>s.
    /// </summary>
    public static VTimeZoneProvider FromTimeZone(IEnumerable<VTimeZone> timeZones)
    {
        var provider = new VTimeZoneProvider();

        foreach (var timeZone in timeZones)
        {
            if (TryCreateIntervals(timeZone, out var tzId, out var intervals))
                provider.Add(tzId, intervals);
        }

        return provider;
    }

    private VTimeZoneProvider()
    {
        _customTimeZones = new();
    }

    private static bool TryCreateIntervals(VTimeZone vTimeZone, out string tzId, out List<ZoneInterval> intervals)
    {
        var timeZoneInfos = vTimeZone.TimeZoneInfos.OrderBy(tzi => tzi.Start.AsUtc).ToList();

        tzId = vTimeZone.TzId;
        intervals = new();

        if (timeZoneInfos.Count == 0)
        {
            return false;
        }

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
            intervals.Add(interval);
        }

        return true;
    }

    /// <summary>
    /// Adds a <see cref="VDateTimeZone"/> to the provider.
    /// </summary>
    /// <param name="tzId">The timezone ID</param>
    /// <param name="intervals">The list of <see cref="ZoneInterval"/>s for the timezone ID.</param>
    private void Add(string tzId, List<ZoneInterval> intervals)
        => Add(new VDateTimeZone(tzId, intervals));

    /// <summary>
    /// Adds a <see cref="VDateTimeZone"/> to the provider if it does not already exist.
    /// </summary>
    /// <param name="timeZone">The <see cref="VDateTimeZone"/> to add.</param>
    /// <returns>This instance.</returns>
    private void Add(DateTimeZone timeZone)
    {
#if NET6_0_OR_GREATER
        _customTimeZones.TryAdd(timeZone.Id, timeZone);
#else
        if (!_customTimeZones.ContainsKey(timeZone.Id))
            _customTimeZones.Add(timeZone.Id, timeZone);
#endif
    }

    /// <inheritdoc/>
    public DateTimeZone GetSystemDefault() => DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <inheritdoc/>
    /// <remarks>
    /// RFC5545 Section 3.6.5: &quot;The VTIMEZONE calendar component is used to define the set of standard time and daylight saving time observances (or rules) for a particular time zone for a given interval of time.&quot;
    /// <para/>
    /// This implies that<br/>
    /// - the VTIMEZONE component is only valid for the time interval covered by its defined transitions. Outside this interval, the behavior is not specified.
    /// - occurrences outside the defined transitions are undefined and may be interpreted differently by different systems.
    /// </remarks>
    public DateTimeZone? GetZoneOrNull(string id)
    {
        // Check for fixed-offset timezones, as described for IDateTimeZoneProvider
        if (id.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            return DateTimeZone.Utc;
        
        if (id.StartsWith("UTC", StringComparison.OrdinalIgnoreCase))
        {
            var parseResult = NodaTime.Text.OffsetPattern.GeneralInvariant.Parse(id.Substring(3));
            if (parseResult.Success)
                return DateTimeZone.ForOffset(parseResult.Value);
        }

        // Check custom timezones
        var found = _customTimeZones.TryGetValue(id, out var timeZone);
        return found ? timeZone : null;
    }

    /// <inheritdoc/>
    public string VersionId => "1.0.0";

    /// <inheritdoc/>
    public ReadOnlyCollection<string> Ids => new(_customTimeZones.Keys.ToList()); // NOSONAR - part of IDateTimeZoneProvider

    /// <inheritdoc/>
    public DateTimeZone this[string id] => GetZoneOrNull(id)!;
}
