//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using NodaTime;

namespace Ical.Net.Utility;

internal static class DateUtil
{
    /// <summary>
    /// Returns the NodaTime DateTimeZone for the given TZID according to the
    /// current time zone resolver set in TimeZoneResolvers.TimeZoneResolver.
    /// </summary>
    /// <param name="tzId">A time zone identifier</param>
    /// <exception cref="ArgumentException">Unrecognized time zone id</exception>
    public static DateTimeZone GetZone(string tzId)
        => TimeZoneResolvers.TimeZoneResolver(tzId) ?? throw new ArgumentException($"Unrecognized time zone id {tzId}", nameof(tzId));

    public static int WeekOfMonth(DateTime d)
    {
        var isExact = d.Day % 7 == 0;
        var offset = isExact
            ? 0
            : 1;
        return (int) Math.Floor(d.Day / 7.0) + offset;
    }
}
