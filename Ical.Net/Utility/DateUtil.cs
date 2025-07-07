//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.DataTypes;
using NodaTime;

namespace Ical.Net.Utility;

internal static class DateUtil
{
    public static CalDateTime AsCalDateTime(this DateTime t)
        => new CalDateTime(t);

    public static CalDateTime AddWeeks(CalDateTime dt, int interval, DayOfWeek firstDayOfWeek)
    {
        dt = dt.AddDays(interval * 7);
        while (dt.DayOfWeek != firstDayOfWeek)
        {
            dt = dt.AddDays(-1);
        }

        return dt;
    }

    public static CalDateTime FirstDayOfWeek(CalDateTime dt, DayOfWeek firstDayOfWeek, out int offset)
    {
        offset = 0;
        while (dt.DayOfWeek != firstDayOfWeek)
        {
            dt = dt.AddDays(-1);
            offset++;
        }
        return dt;
    }

    /// <summary>
    /// Returns the NodaTime DateTimeZone for the given TZID according to the
    /// current time zone resolver set in TimeZoneResolvers.TimeZoneResolver.
    /// </summary>
    /// <param name="tzId">A time zone identifier</param>
    /// <exception cref="ArgumentException">Unrecognized time zone id</exception>
    public static DateTimeZone GetZone(string tzId)
        => TimeZoneResolvers.TimeZoneResolver(tzId) ?? throw new ArgumentException($"Unrecognized time zone id {tzId}", nameof(tzId));

    public static ZonedDateTime AddYears(ZonedDateTime zonedDateTime, int years)
    {
        var futureDate = zonedDateTime.Date.PlusYears(years);
        var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
            zonedDateTime.Second);
        var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
        return zonedFutureDate;
    }

    public static ZonedDateTime AddMonths(ZonedDateTime zonedDateTime, int months)
    {
        var futureDate = zonedDateTime.Date.PlusMonths(months);
        var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
            zonedDateTime.Second);
        var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
        return zonedFutureDate;
    }

    public static ZonedDateTime ToZonedDateTimeLeniently(DateTime dateTime, string tzId)
    {
        var zone = GetZone(tzId);
        var localDt = LocalDateTime.FromDateTime(dateTime);

        // RFC 5545 3.3.5
        // If, based on the definition of the referenced time zone, the local
        // time described occurs more than once(when changing from daylight
        // to standard time), the DATE-TIME value refers to the first
        // occurrence of the referenced time.   Thus, TZID=America/
        // New_York:20071104T013000 indicates November 4, 2007 at 1:30 A.M.
        // EDT (UTC-04:00).  If the local time described does not occur (when
        // changing from standard to daylight time), the DATE-TIME value is
        // interpreted using the UTC offset before the gap in local times.
        // Thus, TZID=America/New_York:20070311T023000 indicates March 11,
        // 2007 at 3:30 A.M. EDT (UTC-04:00), one hour after 1:30 A.M. EST
        // (UTC-05:00).
        var lenientZonedDateTime = localDt.InZoneLeniently(zone).WithZone(zone);

        return lenientZonedDateTime;
    }

    public static ZonedDateTime FromTimeZoneToTimeZone(DateTime dateTime, string fromZoneId, string toZoneId)
        => FromTimeZoneToTimeZone(dateTime, GetZone(fromZoneId), GetZone(toZoneId));

    public static ZonedDateTime FromTimeZoneToTimeZone(DateTime dateTime, DateTimeZone fromZone, DateTimeZone toZone)
    {
        var oldZone = LocalDateTime.FromDateTime(dateTime).InZoneLeniently(fromZone);
        var newZone = oldZone.WithZone(toZone);
        return newZone;
    }

    /// <summary>
    /// Truncate to the specified TimeSpan's magnitude. For example, to truncate to the nearest second, use TimeSpan.FromSeconds(1)
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        => timeSpan == TimeSpan.Zero
            ? dateTime
            : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));

    public static int WeekOfMonth(DateTime d)
    {
        var isExact = d.Day % 7 == 0;
        var offset = isExact
            ? 0
            : 1;
        return (int) Math.Floor(d.Day / 7.0) + offset;
    }

    /// <summary>
    /// Creates an instance that represents the given time span as exact value, that is, time-only.
    /// </summary>
    /// <remarks>
    /// According to RFC5545 the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    internal static DataTypes.Duration ToDurationExact(this TimeSpan timeSpan)
        => DataTypes.Duration.FromTimeSpanExact(timeSpan);

    /// <summary>
    /// Creates an instance that represents the given time span, treating the days as nominal duration and the time part as exact.
    /// </summary>
    /// <remarks>
    /// According to RFC5545 the weeks and day fields of a duration are considered nominal durations while the time fields are considered exact values.
    /// </remarks>
    internal static DataTypes.Duration ToDuration(this TimeSpan timeSpan)
        => DataTypes.Duration.FromTimeSpan(timeSpan);

    internal static CalDateTime AddLeniently(this CalDateTime dt, DataTypes.Duration d)
    {
        if (d.HasTime && !dt.HasTime)
            dt = new CalDateTime(dt.Date, TimeOnly.MinValue);

        return dt.Add(d);
    }
}
