//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// Represents an RFC 5545 VTIMEZONE component.
/// </summary>
public class VTimeZone : CalendarComponent
{
    public static VTimeZone FromLocalTimeZone()
        => FromDateTimeZone(DefaultTimeZoneResolver.LocalDateTimeZone.Id);

    public static VTimeZone FromLocalTimeZone(DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        => FromDateTimeZone(DefaultTimeZoneResolver.LocalDateTimeZone.Id, earliestDateTimeToSupport, includeHistoricalData);

    public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        => FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1), false);

    public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        => FromDateTimeZone(tzinfo.Id, earliestDateTimeToSupport, includeHistoricalData);

    public static VTimeZone FromDateTimeZone(string tzId)
        => FromDateTimeZone(tzId, new DateTime(DateTime.Now.Year, 1, 1), includeHistoricalData: false);

    public static VTimeZone FromDateTimeZone(string tzId, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
    {
        var vTimeZone = new VTimeZone(tzId);

        var earliestYear = 1900;
        var earliestMonth = earliestDateTimeToSupport.Month;
        var earliestDay = earliestDateTimeToSupport.Day;
        // Support date/times for January 1st of the previous year by default.
        if (earliestDateTimeToSupport.Year > 1900)
        {
            earliestYear = earliestDateTimeToSupport.Year - 1;
            // Since we went back a year, we can't still be in a leap-year
            if (earliestMonth == 2 && earliestDay == 29)
                earliestDay = 28;
        }
        else
        {
            // Going back to 1900, which wasn't a leap year, so we need to switch to Feb 20
            if (earliestMonth == 2 && earliestDay == 29)
                earliestDay = 28;
        }
        var earliest = Instant.FromUtc(earliestYear, earliestMonth, earliestDay,
            earliestDateTimeToSupport.Hour, earliestDateTimeToSupport.Minute);

        // Only include historical data if asked to do so.  Otherwise,
        // use only the most recent adjustment rules available.
        var intervals = vTimeZone._nodaZone.GetZoneIntervals(earliest, Instant.FromDateTimeOffset(DateTimeOffset.Now))
            .Where(z => z.HasStart && z.Start != Instant.MinValue)
            .ToList();

        var matchingDaylightIntervals = new List<ZoneInterval>();
        var matchingStandardIntervals = new List<ZoneInterval>();

        // if there are no intervals, create at least one standard interval
        if (!intervals.Any())
        {
            var start = new DateTimeOffset(new DateTime(earliestYear, 1, 1), new TimeSpan(vTimeZone._nodaZone.MaxOffset.Ticks));
            var interval = new ZoneInterval(
                name: vTimeZone._nodaZone.Id,
                start: Instant.FromDateTimeOffset(start),
                end: Instant.FromDateTimeOffset(start) + NodaTime.Duration.FromHours(1),
                wallOffset: vTimeZone._nodaZone.MinOffset,
                savings: Offset.Zero);
            intervals.Add(interval);
            var zoneInfo = CreateTimeZoneInfo(intervals, new List<ZoneInterval>(), true, true);
            vTimeZone.AddChild(zoneInfo);
        }
        else
        {
            // first, get the latest standard and daylight intervals, find the oldest recurring date in both,
            // set the RRULES for it, and create a VTimeZoneInfos out of them.

            // Standard
            var standardIntervals = intervals.Where(x => x.Savings.ToTimeSpan() == new TimeSpan(0)).ToList();
            var latestStandardInterval = standardIntervals.OrderByDescending(x => x.Start).FirstOrDefault();

            if (latestStandardInterval == null)
            {
                return vTimeZone;
            }

            matchingStandardIntervals = GetMatchingIntervals(standardIntervals, latestStandardInterval, true);
            var latestStandardTimeZoneInfo = CreateTimeZoneInfo(matchingStandardIntervals, intervals);
            vTimeZone.AddChild(latestStandardTimeZoneInfo);

            // check to see if there is no active, future daylight savings (ie, America/Phoenix)
            if ((latestStandardInterval.HasEnd ? latestStandardInterval.End : Instant.MaxValue) != Instant.MaxValue)
            {
                //daylight
                var daylightIntervals = intervals.Where(x => x.Savings.ToTimeSpan() != new TimeSpan(0)).ToList();

                if (daylightIntervals.Count != 0)
                {
                    var latestDaylightInterval = daylightIntervals.OrderByDescending(x => x.Start).First();
                    matchingDaylightIntervals =
                        GetMatchingIntervals(daylightIntervals, latestDaylightInterval, true);
                    var latestDaylightTimeZoneInfo = CreateTimeZoneInfo(matchingDaylightIntervals, intervals);
                    vTimeZone.AddChild(latestDaylightTimeZoneInfo);
                }
            }
        }

        if (!includeHistoricalData || intervals.Count == 1)
        {
            return vTimeZone;
        }

        // then, do the historic intervals, using RDATE for them
        var historicIntervals = intervals.Where(x => !matchingDaylightIntervals.Contains(x) && !matchingStandardIntervals.Contains(x)).ToList();

        while (historicIntervals.Any(x => x.Start != Instant.MinValue))
        {
            var interval = historicIntervals.FirstOrDefault(x => x.Start != Instant.MinValue);

            if (interval == null)
            {
                break;
            }

            var matchedIntervals = GetMatchingIntervals(historicIntervals, interval);
            var timeZoneInfo = CreateTimeZoneInfo(matchedIntervals, intervals, false);
            vTimeZone.AddChild(timeZoneInfo);
            historicIntervals = historicIntervals.Where(x => !matchedIntervals.Contains(x)).ToList();
        }

        return vTimeZone;
    }

    private static VTimeZoneInfo CreateTimeZoneInfo(List<ZoneInterval> matchedIntervals, List<ZoneInterval> intervals, bool isRRule = true,
        bool isOnlyInterval = false)
    {
        if (matchedIntervals == null || matchedIntervals.Count == 0)
        {
            throw new InvalidOperationException("No intervals found in matchedIntervals");
        }

        var oldestInterval = matchedIntervals.OrderBy(x => x.Start).FirstOrDefault();
        if (oldestInterval == null)
        {
            throw new InvalidOperationException("oldestInterval was not found");
        }

        var previousInterval = intervals.SingleOrDefault(x => (x.HasEnd ? x.End : Instant.MaxValue) == oldestInterval.Start);

        var delta = new TimeSpan(1, 0, 0);

        if (previousInterval != null)
        {
            delta = new TimeSpan(0, 0, previousInterval.WallOffset.Seconds - oldestInterval.WallOffset.Seconds);
        }
        else if (isOnlyInterval)
        {
            delta = TimeSpan.Zero;
        }

        var utcOffset = oldestInterval.StandardOffset.ToTimeSpan();

        var timeZoneInfo = new VTimeZoneInfo();

        var isDaylight = oldestInterval.Savings.Ticks > 0;

        if (isDaylight)
        {
            timeZoneInfo.Name = Components.Daylight;
            timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset);
            timeZoneInfo.OffsetTo = new UtcOffset(utcOffset - delta);
        }
        else
        {
            timeZoneInfo.Name = Components.Standard;
            timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset + delta);
            timeZoneInfo.OffsetTo = new UtcOffset(utcOffset);
        }

        timeZoneInfo.TimeZoneName = oldestInterval.Name;

        var start = oldestInterval.IsoLocalStart.ToDateTimeUnspecified() + delta;
        timeZoneInfo.Start = new CalDateTime(start, true);

        if (isRRule)
        {
            PopulateTimeZoneInfoRecurrenceRule(timeZoneInfo, oldestInterval);
        }
        else
        {
            PopulateTimeZoneInfoRecurrenceDates(timeZoneInfo, matchedIntervals, delta);
        }

        return timeZoneInfo;
    }

    private static List<ZoneInterval> GetMatchingIntervals(List<ZoneInterval> intervals, ZoneInterval intervalToMatch, bool consecutiveOnly = false)
    {
        var matchedIntervals = intervals
            .Where(x => x.Start != Instant.MinValue)
            .Where(x => x.IsoLocalStart.Month == intervalToMatch.IsoLocalStart.Month
                    && x.IsoLocalStart.Hour == intervalToMatch.IsoLocalStart.Hour
                    && x.IsoLocalStart.Minute == intervalToMatch.IsoLocalStart.Minute
                    && x.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek == intervalToMatch.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek
                    && x.WallOffset == intervalToMatch.WallOffset
                    && x.Name == intervalToMatch.Name)
            .ToList();

        if (!consecutiveOnly)
        {
            return matchedIntervals;
        }

        var consecutiveIntervals = new List<ZoneInterval>();

        var currentYear = 0;

        // return only the intervals where there are no gaps in years
        foreach (var interval in matchedIntervals.OrderByDescending(x => x.IsoLocalStart.Year))
        {
            if (currentYear == 0)
            {
                currentYear = interval.IsoLocalStart.Year;
            }

            if (currentYear != interval.IsoLocalStart.Year)
            {
                break;
            }

            consecutiveIntervals.Add(interval);
            currentYear--;
        }

        return consecutiveIntervals;
    }

    private static void PopulateTimeZoneInfoRecurrenceDates(VTimeZoneInfo tzi, List<ZoneInterval> intervals, TimeSpan delta)
    {
        foreach (var interval in intervals)
        {
            var time = interval.IsoLocalStart.ToDateTimeUnspecified();
            var date = new CalDateTime(time, true).Add(delta.ToDurationExact());

            tzi.RecurrenceDates.Add(date);
        }
    }

    private static void PopulateTimeZoneInfoRecurrenceRule(VTimeZoneInfo tzi, ZoneInterval interval)
    {
        var recurrence = new IntervalRecurrenceRule(interval);
        tzi.RecurrenceRule = recurrence;
    }

    private class IntervalRecurrenceRule : RecurrenceRule
    {
        // Required for serializer
        public IntervalRecurrenceRule() : base() { }

        public IntervalRecurrenceRule(ZoneInterval interval)
        {
            Frequency = FrequencyType.Yearly;
            ByMonth.Add(interval.IsoLocalStart.Month);

            var date = interval.IsoLocalStart.ToDateTimeUnspecified();
            var weekday = date.DayOfWeek;
            var weekNumber = DateUtil.WeekOfMonth(date);

            if (weekNumber >= 4)
                ByDay.Add(new WeekDay(weekday, -1)); // Almost certainly likely last X-day of month. Avoid issues with 4/5 sundays in different year/months. Ideally, use the nodazone tz database rule for this interval instead.
            else
                ByDay.Add(new WeekDay(weekday, weekNumber));
        }
    }

    public VTimeZone()
    {
        Name = Components.Timezone;
    }

    public VTimeZone(string tzId) : this()
    {
        if (string.IsNullOrWhiteSpace(tzId))
        {
            return;
        }

        TzId = tzId;
        Location = _nodaZone.Id;
    }

    private DateTimeZone _nodaZone = DateTimeZone.Utc; // must initialize
    private string? _tzId;
    public virtual string? TzId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_tzId))
            {
                _tzId = Properties.Get<string>("TZID");
            }
            return _tzId;
        }
        set
        {
            if (string.Equals(_tzId, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _tzId = null;
                Properties.Remove("TZID");
                return;
            }

            _nodaZone = DateUtil.GetZone(value);
            var id = _nodaZone.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"Unrecognized time zone id: {value}");
            }

            if (!string.Equals(id, value, StringComparison.OrdinalIgnoreCase))
            {
                //It was a BCL time zone, so we should use the original value
                id = value;
            }

            _tzId = id;
            Properties.Set("TZID", value);
        }
    }

    private Uri? _url;
    public virtual Uri? Url
    {
        get => _url ?? (_url = Properties.Get<Uri>("TZURL"));
        set
        {
            _url = value;
            Properties.Set("TZURL", _url);
        }
    }

    private string? _location;
    public string? Location
    {
        get => _location ??= Properties.Get<string>("X-LIC-LOCATION");
        set
        {
            _location = value;
            Properties.Set("X-LIC-LOCATION", _location);
        }
    }

    public ICalendarObjectList<VTimeZoneInfo> TimeZoneInfos => new CalendarObjectListProxy<VTimeZoneInfo>(Children);
}
