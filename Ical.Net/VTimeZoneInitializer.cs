// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

using System.Collections.Generic;
using Ical.Net.CalendarComponents;

#nullable enable
namespace Ical.Net;

/// <summary>
/// Initializes the <see cref="VTimeZone"/>s in a <see cref="Calendar"/>
/// and adds them to the <see cref="VTimeZoneProvider"/>.
/// </summary>
public static class VTimeZoneInitializer
{
    /// <summary>
    /// Adds the <see cref="VTimeZone"/>s to the <see cref="VTimeZoneProvider"/>.
    /// </summary>
    public static void AddTimeZones(IEnumerable<VTimeZone> timeZones)
    {
        foreach (var timeZone in timeZones)
            new VTimeZoneBuilder(timeZone).Build();
    }

    /// <summary>
    /// Adds <see cref="VTimeZone"/>s in the <see cref="Calendar"/> to the <see cref="VTimeZoneProvider"/>.
    /// </summary>
    public static void AddTimeZones(Calendar calendar)
        => AddTimeZones(calendar.TimeZones);

    /// <summary>
    /// Clears all <see cref="VTimeZone"/>s from the <see cref="VTimeZoneProvider"/>.
    /// </summary>
    public static void ClearTimeZones() => VTimeZoneProvider.Instance.Clear();
}
