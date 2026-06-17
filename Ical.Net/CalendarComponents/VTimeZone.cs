//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// Represents an RFC 5545 VTIMEZONE component.
/// </summary>
public class VTimeZone : CalendarComponent
{
    public static VTimeZone FromLocalTimeZone()
        => FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault().Id);

    public static VTimeZone FromLocalTimeZone(DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        => FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault().Id, earliestDateTimeToSupport, includeHistoricalData);

    public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        => FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1), false);

    public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        => FromDateTimeZone(tzinfo.Id, earliestDateTimeToSupport, includeHistoricalData);

    public static VTimeZone FromDateTimeZone(string tzId)
        => FromDateTimeZone(tzId, new DateTime(DateTime.Now.Year, 1, 1), includeHistoricalData: false);

    public static VTimeZone FromDateTimeZone(string tzId, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
    {
        var tz = CalendarTimeZoneProviders.TzdbWithAliases[tzId];
        return FromDateTimeZone(tz, tzId, earliestDateTimeToSupport, includeHistoricalData);
    }

    public static VTimeZone FromDateTimeZone(DateTimeZone tz)
        => FromDateTimeZone(tz, new DateTime(DateTime.Now.Year, 1, 1), includeHistoricalData: false);

    public static VTimeZone FromDateTimeZone(DateTimeZone tz, DateTime earliestDateTimeToSupport, bool includeHistoricalData)
        => FromDateTimeZone(tz, tz.Id, earliestDateTimeToSupport, includeHistoricalData);

    internal static VTimeZone FromDateTimeZone(
        DateTimeZone tz,
        string timeZoneAlias,
        DateTime earliestDateTimeToSupport,
        bool includeHistoricalData)
    {
        // Use the alias as the VTIMEZONE TZID value
        var vTimeZone = new VTimeZone(tz, timeZoneAlias);

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
        var intervals = tz.GetZoneIntervals(earliest, Instant.FromDateTimeOffset(DateTimeOffset.Now))
            .Where(z => z.HasStart && z.Start != Instant.MinValue)
            .ToList();

        var matchingDaylightIntervals = new List<ZoneInterval>();
        var matchingStandardIntervals = new List<ZoneInterval>();

        // if there are no intervals, create at least one standard interval
        if (intervals.Count == 0)
        {
            var start = new DateTimeOffset(new DateTime(earliestYear, 1, 1), new TimeSpan(tz.MaxOffset.Ticks));
            var interval = new ZoneInterval(
                name: tz.Id,
                start: Instant.FromDateTimeOffset(start),
                end: Instant.FromDateTimeOffset(start) + NodaTime.Duration.FromHours(1),
                wallOffset: tz.MinOffset,
                savings: Offset.Zero);
            intervals.Add(interval);
            var zoneInfo = CreateTimeZoneInfo(intervals, [], true, true);
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
        timeZoneInfo.Start = CalDateTime.FromDateTime(start);

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
            var time = interval.IsoLocalStart.PlusSeconds((long)delta.TotalSeconds);
            var date = new CalDateTime(time);

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

            var weekday = interval.IsoLocalStart.DayOfWeek.ToDayOfWeek();
            var weekNumber = WeekOfMonth(interval.IsoLocalStart);

            if (weekNumber >= 4)
                ByDay.Add(new WeekDay(weekday, -1)); // Almost certainly likely last X-day of month. Avoid issues with 4/5 sundays in different year/months. Ideally, use the nodazone tz database rule for this interval instead.
            else
                ByDay.Add(new WeekDay(weekday, weekNumber));
        }

        private static int WeekOfMonth(LocalDateTime d)
        {
            var isExact = d.Day % 7 == 0;
            var offset = isExact ? 0 : 1;
            return (int) Math.Floor(d.Day / 7.0) + offset;
        }
    }

    public VTimeZone()
    {
        Name = Components.Timezone;
    }

    /// <summary>
    /// Creates a VTIMEZONE component using the specified time zone ID.
    /// <para/>
    /// It is recommended to use an ID from <see cref="DateTimeZoneProviders.Tzdb"/>.
    /// <para/>
    /// The <see cref="Location"/> will be set if the time zone ID matches
    /// a time zone in <see cref="DateTimeZoneProviders.Tzdb"/>.
    /// </summary>
    /// <param name="tzId">The TZID property value.</param>
    public VTimeZone(string tzId) : this()
    {
        if (string.IsNullOrWhiteSpace(tzId))
        {
            return;
        }

        TzId = tzId;

        // Time zone ID could be a standard ID or a custom one
        Location = CalendarTimeZoneProviders.TzdbWithAliases.GetZoneOrNull(tzId)?.Id;
    }

    internal VTimeZone(DateTimeZone tz, string timeZoneAlias)
    {
        TzId = timeZoneAlias;
        Location = tz.Id;
    }

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

            _tzId = value;
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

    internal DateTimeZone ToDateTimeZone() => CalendarDateTimeZone.From(this);

    private sealed class CalendarDateTimeZone : DateTimeZone
    {
        private readonly List<VTimeZoneInfo> intervals;

        public static CalendarDateTimeZone From(VTimeZone data)
        {
            if (data.TzId == null)
            {
                throw new Exception("Time zone ID must be set");
            }

            var info = data.TimeZoneInfos
                .Select(x => x.Copy<VTimeZoneInfo>()!)
                .ToList();

            var min = Offset.Zero;
            var max = Offset.Zero;

            void CheckMinMax(UtcOffset? utcOffset)
            {
                if (utcOffset != null)
                {
                    var from = Offset.FromTimeSpan(utcOffset.Offset);
                    if (from < min)
                    {
                        min = from;
                    }

                    if (from > max)
                    {
                        max = from;
                    }
                }
            }

            foreach (var x in info)
            {
                CheckMinMax(x.OffsetFrom);
                CheckMinMax(x.OffsetTo);
            }

            var tz = new CalendarDateTimeZone(data.TzId, false, min, max, info);

            return tz;
        }

        private CalendarDateTimeZone(string id, bool isFixed, Offset minOffset, Offset maxOffset, List<VTimeZoneInfo> intervals)
            : base(id, isFixed, minOffset, maxOffset)
        {
            this.intervals = intervals;
        }

        public override ZoneInterval GetZoneInterval(Instant instant)
        {
            Instant? start = null;
            Instant? end = null;
            VTimeZoneInfo? info = null;

            var previousYear = instant.InUtc()
                .LocalDateTime
                .PlusYears(-1)
                .InUtc()
                .ToInstant();

            var timeZoneChanges = CollectionHelpers.OrderedMergeMany(
                intervals.Select(x => x.GetOccurrences(Utc, previousYear)));

            foreach (var occurrence in timeZoneChanges)
            {
                var changeInstant = occurrence.Start.ToInstant();

                if (changeInstant > instant)
                {
                    end = changeInstant;
                    break;
                }
                else
                {
                    info = (VTimeZoneInfo) occurrence.Source;
                    start = changeInstant;
                }
            }

            if (info == null)
            {
                // Fixed offset time zone
                var maxStart = Instant.MinValue;
                var fixedTimeZones = intervals.Where(x => x.RecurrenceRule == null);

                foreach (var tz in fixedTimeZones)
                {
                    var tzStart = tz.Start!.ToLocalDateTime().InUtc().ToInstant();
                    if (tzStart > maxStart && tzStart <= instant)
                    {
                        maxStart = tzStart;
                        info = tz;
                    }
                }
            }

            if (info == null)
            {
                throw new Exception("Could not find zone interval");
            }

            var name = info.TimeZoneName ?? "Unknown";

            var wallOffset = Offset.FromTimeSpan(info.OffsetTo!.Offset);

            var savings = info.Name == Components.Standard
                ? Offset.Zero
                : Offset.FromTimeSpan(info.OffsetTo!.Offset) - wallOffset;

            return new ZoneInterval(name, start, end, wallOffset, savings);
        }
    }
}
