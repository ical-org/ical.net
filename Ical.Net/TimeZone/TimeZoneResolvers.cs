//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using NodaTime;

namespace Ical.Net.TimeZone;

public static class TimeZoneResolvers
{
    /// <summary>
    /// The default time zone resolver.
    /// </summary>
    public static Func<string, DateTimeZone> Default
        => tzId => DefaultTimeZoneResolver.GetZone(tzId) ?? throw new ArgumentException($"Unrecognized time zone id {tzId}", nameof(tzId));

    /// <summary>
    /// Gets or sets a function that returns the NodaTime DateTimeZone for the given TZID.
    /// </summary>
    public static Func<string, DateTimeZone> TimeZoneResolver { get; set; } = Default;
}
