//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
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

    /// <summary>
    /// Sets the day of the month to the target day
    /// or the nearest day of the month.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetDay"></param>
    /// <returns></returns>
    internal static LocalDate AtNearestDayOfMonth(this LocalDate value, int targetDay)
    {
        if (value.Day == targetDay)
        {
            return value;
        }

        var year = value.Year;
        var month = value.Month;
        var daysInMonth = value.Calendar.GetDaysInMonth(year, month);

        return new LocalDate(year, month, Math.Min(targetDay, daysInMonth));
    }

    /// <summary>
    /// Returns the same date if the day of week is already
    /// the target day of week, else the next date matching
    /// the target day of week.
    /// </summary>
    /// <param name="value">Start date</param>
    /// <param name="targetDayOfWeek">Target day of week</param>
    /// <returns></returns>
    internal static LocalDate CurrentOrNext(this LocalDate value, IsoDayOfWeek targetDayOfWeek)
    {
        if (value.DayOfWeek == targetDayOfWeek)
        {
            return value;
        }

        return value.Next(targetDayOfWeek);
    }
}
