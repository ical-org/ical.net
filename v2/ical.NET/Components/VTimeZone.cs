using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using NodaTime;
using NodaTime.TimeZones;
using Period = NodaTime.Period;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        public static VTimeZone FromLocalTimeZone()
        {
            return FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromDateTimeZone(DateTimeZoneProviders.Tzdb.GetSystemDefault(), earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var zone = DateTimeZoneProviders.Bcl.GetZoneOrNull(tzinfo.Id);
            
            return FromDateTimeZone(zone, earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromDateTimeZone(DateTimeZone zone)
        {
            return FromDateTimeZone(zone, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static VTimeZone FromDateTimeZone(DateTimeZone zone, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var vTimeZone = new VTimeZone(zone.Id);
            var earliestYear = 1900;
            if (earlistDateTimeToSupport.Year > 1900)
                earliestYear = earlistDateTimeToSupport.Year - 1;
            var earliest = Instant.FromUtc(earliestYear, earlistDateTimeToSupport.Month,
                earlistDateTimeToSupport.Day, earlistDateTimeToSupport.Hour, earlistDateTimeToSupport.Minute);

            // Only include historical data if asked to do so.  Otherwise,
            // use only the most recent adjustment rules available.
            var intervals = zone.GetZoneIntervals(earliest.Minus(Duration.FromStandardDays(365)), SystemClock.Instance.Now).Where(x => x.Start != Instant.MinValue).ToList();

            // first, get the latest standard and daylight intervals, find the oldest recurring date in both, set the RRULES for it, and create a VTimeZoneInfos out of them.
            //standard
            var standardIntervals = intervals.Where(x => x.Savings.ToTimeSpan() == new TimeSpan(0)).ToList();
            var latestStandardInterval = standardIntervals.OrderByDescending(x => x.Start).FirstOrDefault();
            List<ZoneInterval> matchingStandardIntervals =
                GetMatchingIntervals(standardIntervals, latestStandardInterval, true);
            var latestStandardTimeZoneInfo = CreateTimeZoneInfo(zone, matchingStandardIntervals, intervals, earliest);
            vTimeZone.AddChild(latestStandardTimeZoneInfo);

            //daylight
            var daylightIntervals = intervals.Where(x => x.Savings.ToTimeSpan() != new TimeSpan(0)).ToList();
            var latestDaylightInterval = daylightIntervals.OrderByDescending(x => x.Start).FirstOrDefault();
            List<ZoneInterval> matchingDaylightIntervals =
                GetMatchingIntervals(daylightIntervals, latestDaylightInterval, true);
            var latestDaylightTimeZoneInfo = CreateTimeZoneInfo(zone, matchingDaylightIntervals, intervals, earliest);
            vTimeZone.AddChild(latestDaylightTimeZoneInfo);

            if (includeHistoricalData)
            {
                // then, do the historic intervals, using RDATE for them
                var historicIntervals = intervals.Where(x => !matchingDaylightIntervals.Contains(x) && !matchingStandardIntervals.Contains(x)).ToList();

                while (historicIntervals.Any(x=>x.Start != Instant.MinValue))
                {
                    var interval = historicIntervals.FirstOrDefault();
                    if (interval == null || interval.Start == Instant.MinValue)
                    {
                        continue;
                    }
                        
                    var matchedIntervals = GetMatchingIntervals(historicIntervals, interval);
                    var timeZoneInfo = CreateTimeZoneInfo(zone, matchedIntervals, intervals, earliest, false);
                    vTimeZone.AddChild(timeZoneInfo);
                    historicIntervals = historicIntervals.Where(x => !matchedIntervals.Contains(x)).ToList();
                }
            }

            return vTimeZone;
        }

        private static ITimeZoneInfo CreateTimeZoneInfo(DateTimeZone zone, List<ZoneInterval> matchedIntervals, List<ZoneInterval> intervals, Instant earliest, bool isRRule = true)
        {
            var oldestInterval = matchedIntervals.OrderBy(x => x.Start).FirstOrDefault();

            var previousInterval = intervals.SingleOrDefault(x => x.End == oldestInterval.Start);
            var delta = new TimeSpan(1,0,0);
            if (previousInterval != null)
            {
                delta = (intervals.SingleOrDefault(x => x.End == oldestInterval.Start).WallOffset -
                         oldestInterval.WallOffset).ToTimeSpan();
            }
           
            var utcOffset = oldestInterval.StandardOffset.ToTimeSpan();

            ITimeZoneInfo timeZoneInfo = new VTimeZoneInfo();

            var isDaylight = oldestInterval.Savings.Ticks > 0;

            if (isDaylight)
            {
                timeZoneInfo.Name = "Daylight";
                timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset);
                timeZoneInfo.OffsetTo = new UtcOffset(utcOffset - delta);
            }
            else
            {
                timeZoneInfo.Name = "STANDARD";
                timeZoneInfo.OffsetFrom = new UtcOffset(utcOffset + delta);
                timeZoneInfo.OffsetTo = new UtcOffset(utcOffset);
            }

            timeZoneInfo.TimeZoneName = oldestInterval.Name;

            var start = oldestInterval.IsoLocalStart.ToDateTimeUnspecified() + delta;
            timeZoneInfo.Start = new CalDateTime(start);

            //if (start < earliest.InZone(zone).ToDateTimeUnspecified())
              //  timeZoneInfo.Start = timeZoneInfo.Start.AddYears(earliest.InZone(zone).Year - timeZoneInfo.Start.Year);

            if(isRRule)
                PopulateTimeZoneInfoRecurrenceRules(timeZoneInfo, oldestInterval);
            else
            {
                PopulateTimeZoneInfoRecurrenceDates(timeZoneInfo, matchedIntervals, delta);
            }

            return timeZoneInfo;
        }

        private static List<ZoneInterval> GetMatchingIntervals(List<ZoneInterval> intervals, ZoneInterval intervalToMatch, bool consecutiveOnly = false)
        {
            var matchedIntervals = intervals.Where(x=>x.Start != Instant.MinValue)
                .Where(x => x.IsoLocalStart.Month == intervalToMatch.IsoLocalStart.Month 
                            && x.IsoLocalStart.Hour == intervalToMatch.IsoLocalStart.Hour
                            && x.IsoLocalStart.Minute == intervalToMatch.IsoLocalStart.Minute
                            && x.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek == intervalToMatch.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek
                            && x.WallOffset == intervalToMatch.WallOffset)
                .ToList();


            if (!consecutiveOnly)
                return matchedIntervals;

            var consecutiveIntervals = new List<ZoneInterval>();

            var currentYear = 0;

            // return only the intervals where there are no gaps in years
            foreach (var interval in matchedIntervals.OrderBy(x=>x.IsoLocalStart.Year))
            {
                if (currentYear == 0)
                    currentYear = interval.IsoLocalStart.Year;

                if (currentYear != interval.IsoLocalStart.Year)
                    break;
                
                consecutiveIntervals.Add(interval);
                currentYear++;
            }

            return consecutiveIntervals;
        }

        private class IntervalRecurrencePattern : RecurrencePattern
        {
            public IntervalRecurrencePattern(ZoneInterval interval) 
            {
                Frequency = FrequencyType.Yearly;
                ByMonth.Add(interval.IsoLocalStart.Month);

                var weekday = interval.IsoLocalStart.ToDateTimeUnspecified().DayOfWeek;
                ByDay.Add(new WeekDay(weekday, 1));
            }

        }

        private static void PopulateTimeZoneInfoRecurrenceDates(ITimeZoneInfo tzi, List<ZoneInterval> intervals, TimeSpan delta)
        {
            foreach (var interval in intervals)
            {
                var periodList = new PeriodList();
                var date = new CalDateTime(interval.IsoLocalStart.ToDateTimeUnspecified());
                periodList.Add(date.Add(delta));
                tzi.RecurrenceDates.Add(periodList);
            }
            
        }

        private static void PopulateTimeZoneInfoRecurrenceRules(ITimeZoneInfo tzi, ZoneInterval interval)
        {
            var recurrence = new IntervalRecurrencePattern(interval);
            tzi.RecurrenceRules.Add(recurrence);
        }

        public VTimeZone()
        {
            Name = Components.Timezone;
        }
        
        public VTimeZone(string tzId) : this()
        {
            if (string.IsNullOrEmpty(tzId))
            {
                return;
            }
            
            var zone = DateTimeZoneProviders.Bcl.Ids.SingleOrDefault(x => x == tzId);

            if (string.IsNullOrEmpty(zone))
            {
                zone = DateTimeZoneProviders.Tzdb.Ids.SingleOrDefault(x => x == tzId);
                
                if (string.IsNullOrEmpty(zone))
                {
                    throw new ArgumentException($"The tzId ({zone}) is not a valid timezone");
                }
            }

            TzId = tzId;
        }

        private string _tzId;
        public virtual string TzId
        {
            get
            {
                if (string.IsNullOrEmpty(_tzId))
                {
                    _tzId = Properties.Get<string>("TZID");
                }
                return _tzId;
            }
            set
            {
                _tzId = value;
                Properties.Set("TZID", _tzId);
            }
        }

        public IDateTime LastModified { get; set; }
        public Uri TZUrl { get; set; }

        private Uri _url;

        public virtual Uri Url
        {
            get { return _url ?? (_url = Properties.Get<Uri>("TZURL")); }
            set
            {
                _url = value;
                Properties.Set("TZURL", _url);
            }
        }

        public HashSet<ITimeZoneInfo> TimeZoneInfos { get; set; }

        protected bool Equals(VTimeZone other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(TzId, other.TzId, StringComparison.OrdinalIgnoreCase)
                && Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VTimeZone)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (TzId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Url?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

       
    }
}