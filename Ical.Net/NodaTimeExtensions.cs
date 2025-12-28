//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;
internal static class NodaTimeExtensions
{
    /// <summary>
    /// Returns a ZonedDateTime that is matches the time zone and
    /// offset of the start value, or shifts forward if the local
    /// time does not exist.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    internal static ZonedDateTime InZoneRelativeTo(this LocalDateTime value, ZonedDateTime start)
    {
        var map = start.Zone.MapLocal(value);

        if (map.Count == 1)
        {
            return map.Single();
        }
        else if (map.Count == 2)
        {
            // Only map forward in time
            var last = map.Last();
            if (last.Offset == start.Offset)
            {
                return last;
            }
            else
            {
                return map.First();
            }
        }
        else
        {
            // Invalid local time, shift forward
            return Resolvers.ReturnForwardShifted
                .Invoke(map.LocalDateTime, map.Zone, map.EarlyInterval, map.LateInterval);
        }
    }
}
